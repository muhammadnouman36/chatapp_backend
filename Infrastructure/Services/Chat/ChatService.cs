using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Chat;
using Application.VMs;
using Application.VMs.Token;
using Domain.Chat;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Chat
{
    public class ChatService : IChat
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private static readonly Dictionary<long, string> _userConnections = new();

        public ChatService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public ResponseVM GetAllUsers()
        {
            ResponseVM response = ResponseVM.Instance;

            var users = _context.AppUser.Where(u => !u.IsDeleted).ToList();

            if(users == null)
            {
                response.responseCode = 400;
                response.responseMessage = "No Users Found";
               
            }
            else
            {
                response.responseCode = 200;
                response.responseMessage = "All Users Retrived";
                response.data = users;
                
            }

            return response;

        }



        public ResponseVM GetFriends()
        {
            var response = ResponseVM.Instance;
            var currentUserId = TokenVm.UserID;

            var friends = _context.Friends
                .Where(f => f.UserId == currentUserId && !f.isUnfriend)
                .Include(f => f.appuserid2)
                .ToList();

            if (friends == null || !friends.Any())
            {
                response.responseCode = 400;
                response.responseMessage = "No Friends";
                response.data = null;
                return response;
            }
            var friendUsers = friends.Select(f => f.appuserid2).ToList();

            response.responseCode = 200;
            response.responseMessage = "Friends Retrieved";
            response.data = friendUsers; 
            return response;
        }

        public void AddUser(long userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
        }


        public ResponseVM AddFriend(long friendId)
        {
            var response = ResponseVM.Instance;
            var currentUserId = TokenVm.UserID;

            var existingFriendship = _context.Friends
                .FirstOrDefault(f => f.UserId == currentUserId && f.FriendId == friendId);

            if (existingFriendship != null)
            {
                response.responseCode = 400;
                response.responseMessage = "Already friend";
                return response;
            }

            var friend = new Friends
            {
                UserId = currentUserId,
                FriendId = friendId,
                AddedAt = DateTime.UtcNow
            };

            var friend2 = new Friends
            {
                UserId = friendId,
                FriendId = currentUserId,
                AddedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friend);
            _context.Friends.Add(friend2);
            _context.SaveChanges();
            response.responseCode = 200;
            response.responseMessage = "Added";
            return response;
        }

        public string? GetConnectionId(long userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        public void RemoveUser(string connectionId)
        {
            var item = _userConnections.FirstOrDefault(x => x.Value == connectionId);
            if (!item.Equals(default(KeyValuePair<long, string>)))
            {
                _userConnections.Remove(item.Key);
            }
        }

        public IEnumerable<string> GetAllConnectionIds()
        {
            return _userConnections.Values;
        }
    }
}
