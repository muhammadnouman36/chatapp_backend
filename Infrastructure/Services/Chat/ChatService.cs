using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Chat;
using Application.VMs;
using Infrastructure.Context;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Chat
{
    public class ChatService : IChat
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

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
    }
}
