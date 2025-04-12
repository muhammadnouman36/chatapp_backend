using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.AppUsers;
using Infrastructure.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.SignalR
{
    public class ChatHub : Hub
    {

        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public ChatHub(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }


        //public async Task SendMessage(string senderId, string receiverId, string content)
        //{
        //    var message = new Messages
        //    {
        //        SenderId = int.Parse(senderId),
        //        ReceiverId = int.Parse(receiverId),
        //        Content = content,
        //        Timestamp = DateTime.UtcNow,
        //        IsRead = false
        //    };

        //    // Save the message to the database
        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    // Now, broadcast the message to the receiving client
        //    await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, receiverId, content);
        //    await Clients.Group(senderId).SendAsync("ReceiveMessage", senderId, receiverId, content);  // Optional: Broadcast back to the sender
        //}


        // Send a message to all connected clients
        public async Task SendMessage(long user, string message)
        {
            // "ReceiveMessage" is the method clients will listen to
            await Clients.All.SendAsync("ReceiveMessage", user, message);
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
    }
}
