using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Application.VMs.Chat
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Use your custom claim "ID" as user identifier
            return connection.User?.FindFirst("ID")?.Value;
        }
    }
}
