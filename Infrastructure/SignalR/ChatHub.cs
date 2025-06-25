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
            //_chatService = chatservice;
        }


        //public async Task SendMessage(long user, string message,string userName , string timestamp)
        //{
        //    try
        //    {
        //        await Clients.All.SendAsync("ReceiveMessage", user, message, userName, timestamp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error sending message: " + ex.Message);
        //        throw; 
        //    }
        //}


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
            // Store username in Context.Items (optional)
            Context.Items["username"] = username;

            var connectionId = Context.ConnectionId;

            // Try to find a partner
            var partner = WaitingUsers.FirstOrDefault(u => u.Key != connectionId);

            if (partner.Key != null)
            {
                // Pair them
                WaitingUsers.Remove(partner.Key);
                PairedUsers[connectionId] = partner.Key;
                PairedUsers[partner.Key] = connectionId;

                await Clients.Client(connectionId).SendAsync("PartnerFound", WaitingUsers[partner.Key]);
                await Clients.Client(partner.Key).SendAsync("PartnerFound", WaitingUsers[connectionId]);
            }
            else
            {
                WaitingUsers[connectionId] = username;
            }
        }

        public async Task SendRandomMessage(string message)
        {
            if (PairedUsers.TryGetValue(Context.ConnectionId, out var partnerId))
            {
                await Clients.Client(partnerId).SendAsync("ReceiveMessage", message);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
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

            return base.OnDisconnectedAsync(exception);
        }


    }
}
