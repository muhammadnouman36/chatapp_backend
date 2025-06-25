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

            // Try to find a random partner
            var potentialPartners = WaitingUsers
                .Where(u => u.Key != connectionId)
                .OrderBy(_ => Guid.NewGuid()) // Randomize selection
                .ToList();

            if (potentialPartners.Any())
            {
                var partner = potentialPartners.First();
                var partnerId = partner.Key;
                var partnerUsername = partner.Value;

                // Remove both from waiting list
                WaitingUsers.Remove(partnerId);

                // Add to paired users
                PairedUsers[connectionId] = partnerId;
                PairedUsers[partnerId] = connectionId;

                // Notify both users
                await Clients.Client(connectionId).SendAsync("PartnerFound", partnerUsername);
                await Clients.Client(partnerId).SendAsync("PartnerFound", username);
            }
            else
            {
                WaitingUsers[connectionId] = username;
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

            // Unpair
            if (PairedUsers.TryGetValue(connectionId, out var partnerId))
            {
                PairedUsers.Remove(partnerId);
                PairedUsers.Remove(connectionId);
                _ = Clients.Client(partnerId).SendAsync("PartnerLeft");
            }

            // Remove from waiting queue
            if (WaitingUsers.ContainsKey(connectionId))
            {
                WaitingUsers.Remove(connectionId);
            }
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




    }
}
