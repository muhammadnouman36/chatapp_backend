using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Domain.AppUsers;
using Infrastructure.Context;
using Infrastructure.Services.Chat;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.SignalR
{
    public class ChatHub : Hub
    {

        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        //private readonly ChatService _chatService;

        public ChatHub(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
            //_chatService = chatservice;
        }




        // Send a message to all connected clients
        public async Task SendMessage(long user, string message,string userName , string timestamp)
        {
            // "ReceiveMessage" is the method clients will listen to
            try
            {
                await Clients.All.SendAsync("ReceiveMessage", user, message, userName, timestamp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
                throw; // rethrow to see exact cause in logs
            }
        }

        // You can also create group chats or individual message features
        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            // Send a message to all clients in a specific group
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        // Join a group (for creating chat rooms)
        public async Task JoinGroup(string groupName)

        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }


        //public override Task OnConnectedAsync()
        //{
        //    if (long.TryParse(Context.GetHttpContext()?.Request.Query["userId"], out long userId))
        //    {
        //        _chatService.AddUser(userId, Context.ConnectionId);
        //    }
        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception? exception)
        //{
        //    _chatService.RemoveUser(Context.ConnectionId);
        //    return base.OnDisconnectedAsync(exception);
        //}

        //public async Task SendPrivateMessage(long toUserId, string senderName, string message)
        //{
        //    var receiverConnectionId = _chatService.GetConnectionId(toUserId);
        //    if (!string.IsNullOrEmpty(receiverConnectionId))
        //    {
        //        await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", senderName, message);
        //    }
        //}

    }
}
