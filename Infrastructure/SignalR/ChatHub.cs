using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.VMs.Token;
using Application.VMs;
using Domain.AppUsers;
using Infrastructure.Context;
using Infrastructure.Services.Chat;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using Application.VMs.Chat.RandomChat;

namespace Infrastructure.SignalR
{
    public class ChatHub : Hub
    {

        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, string> WaitingUsers = new();
        private static readonly Dictionary<string, string> PairedUsers = new();
        //private readonly ChatService _chatService;

        public ChatHub(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }
        public async Task SendPrivateMessage(long receiverId, string messageText)
        {
            var senderId = TokenVm.UserID;
            if (senderId == 0)
                throw new HubException("Unauthorized");

            var areFriends = _context.Friends.Any(f =>
                (f.UserId == senderId && f.FriendId == receiverId && !f.isBloacked && !f.isUnfriend) ||
                (f.UserId == receiverId && f.FriendId == senderId && !f.isBloacked && !f.isUnfriend));

            if (!areFriends)
                throw new HubException("Not friends");

            var message = new Messages
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = messageText,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", message);
            await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", message);
        }
        public async Task RegisterUser(string username)
        {
            var connectionId = Context.ConnectionId;

            // Remove stale data
            WaitingUsers.Remove(connectionId);
            PairedUsers.Remove(connectionId);

            Context.Items["username"] = username;

            // Try to find a random partner (excluding self-pairing by connectionId and username)
            var potentialPartners = WaitingUsers
                .Where(u => u.Key != connectionId && u.Value != username) // avoid matching with self
                .OrderBy(_ => Guid.NewGuid()) // random
                .ToList();

            if (potentialPartners.Any())
            {
                var partner = potentialPartners.First();
                var partnerId = partner.Key;
                var partnerUsername = partner.Value;

                // Remove partner from waiting
                WaitingUsers.Remove(partnerId);

                // Add both users to paired dictionary
                PairedUsers[connectionId] = partnerId;
                PairedUsers[partnerId] = connectionId;

                // Notify both users
                await Clients.Client(connectionId).SendAsync("PartnerFound", partnerUsername);
                await Clients.Client(partnerId).SendAsync("PartnerFound", username);
            }
            else
            {
                // No partner found, add to waiting and notify
                WaitingUsers[connectionId] = username;
                await Clients.Client(connectionId).SendAsync("NoPartnerAvailable");
            }

            await BroadcastStats();
        }
        public async Task SendRandomMessage(string message)
        {
            if (PairedUsers.TryGetValue(Context.ConnectionId, out var partnerId))
            {
                await Clients.Client(partnerId).SendAsync("ReceiveMessage", message);
            }
        }
        public override async Task OnConnectedAsync()
        {
            await BroadcastStats();
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            // Step 1: Handle paired user
            if (PairedUsers.TryGetValue(connectionId, out var partnerId))
            {
                // Remove both from paired dictionary
                PairedUsers.Remove(connectionId);
                PairedUsers.Remove(partnerId);

                // Inform the user that their partner left
                await Clients.Client(partnerId).SendAsync("PartnerLeft");

                // Try to find a new match for the partner
                if (WaitingUsers.Count > 0)
                {
                    // Get a new match for the partner
                    var nextWaitingUser = WaitingUsers
                        .Where(u => u.Key != partnerId)
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(nextWaitingUser.Key))
                    {
                        WaitingUsers.Remove(nextWaitingUser.Key);

                        // Add both to paired users
                        PairedUsers[partnerId] = nextWaitingUser.Key;
                        PairedUsers[nextWaitingUser.Key] = partnerId;

                        // Notify both clients
                        var partnerUsername = Context.Items.ContainsKey("username") ? Context.Items["username"]?.ToString() : "User";
                        var newUsername = nextWaitingUser.Value;

                        await Clients.Client(partnerId).SendAsync("PartnerFound", newUsername);
                        await Clients.Client(nextWaitingUser.Key).SendAsync("PartnerFound", partnerUsername);
                    }
                    else
                    {
                        // No available match, add to waiting
                        WaitingUsers[partnerId] = "ReconnectedUser"; // You can pass username if stored
                        await Clients.Client(partnerId).SendAsync("NoPartnerAvailable");
                    }
                }
                else
                {
                    // No waiting users at all
                    WaitingUsers[partnerId] = "ReconnectedUser";
                    await Clients.Client(partnerId).SendAsync("NoPartnerAvailable");
                }
            }

            // Step 2: Remove from waiting list if not already handled
            WaitingUsers.Remove(connectionId);

            await BroadcastStats();
            await base.OnDisconnectedAsync(exception);
        }
        private async Task BroadcastStats()
        {
            var stats = new RandomChatCount
            {
                TotalConnectedUsers = WaitingUsers.Count + PairedUsers.Count,
                WaitingUsersCount = WaitingUsers.Count,
                PairedUsersCount = PairedUsers.Count / 2
            };

            await Clients.All.SendAsync("ConnectionStatsUpdated", stats);
        }
        public async Task SkipAndConnectNext()
        {
            var connectionId = Context.ConnectionId;

            // Get current username
            var username = Context.Items.ContainsKey("username") ? Context.Items["username"]?.ToString() ?? "User" : "User";

            // Disconnect current partner (if any)
            string previousPartnerId = null;
            if (PairedUsers.TryGetValue(connectionId, out var currentPartnerId))
            {
                // Save previous partner
                previousPartnerId = currentPartnerId;

                // Remove both from paired
                PairedUsers.Remove(connectionId);
                PairedUsers.Remove(currentPartnerId);

                // Notify current partner
                await Clients.Client(currentPartnerId).SendAsync("PartnerLeft");

                // Temporarily move current partner to waiting list
                if (!WaitingUsers.ContainsKey(currentPartnerId))
                {
                    WaitingUsers[currentPartnerId] = "ReconnectedUser";
                }
            }

            // Remove current user from waiting
            WaitingUsers.Remove(connectionId);

            // Try to find a new partner (excluding current and previous)
            var potentialPartners = WaitingUsers
                .Where(u => u.Key != connectionId && u.Key != previousPartnerId)
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            if (potentialPartners.Any())
            {
                // New partner found
                var newPartner = potentialPartners.First();
                WaitingUsers.Remove(newPartner.Key);

                PairedUsers[connectionId] = newPartner.Key;
                PairedUsers[newPartner.Key] = connectionId;

                await Clients.Client(connectionId).SendAsync("PartnerFound", newPartner.Value);
                await Clients.Client(newPartner.Key).SendAsync("PartnerFound", username);
            }
            else if (!string.IsNullOrEmpty(previousPartnerId))
            {
                // No new partner found, try to reconnect to previous partner
                if (WaitingUsers.ContainsKey(previousPartnerId))
                {
                    WaitingUsers.Remove(previousPartnerId);

                    PairedUsers[connectionId] = previousPartnerId;
                    PairedUsers[previousPartnerId] = connectionId;

                    var prevUsername = "PreviousUser";
                    if (Context.Items.ContainsKey("username"))
                    {
                        prevUsername = Context.Items["username"]?.ToString() ?? "User";
                    }

                    await Clients.Client(connectionId).SendAsync("PartnerFound", prevUsername);
                    await Clients.Client(previousPartnerId).SendAsync("PartnerFound", username);
                }
                else
                {
                    // Previous partner not available anymore
                    WaitingUsers[connectionId] = username;
                    await Clients.Client(connectionId).SendAsync("NoPartnerAvailable");
                }
            }
            else
            {
                // No partner available
                WaitingUsers[connectionId] = username;
                await Clients.Client(connectionId).SendAsync("NoPartnerAvailable");
            }

            await BroadcastStats();
        }

        public Task<bool> CheckUsernameExists(string username)
        {
            bool exists = WaitingUsers.Any(u => u.Value == username) || PairedUsers.Any(u => u.Value == username);
            return Task.FromResult(exists);
        }






    }
}
