using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.VMs.Chat.RandomChat
{
    public class RandomChatCount
    {
        public int TotalConnectedUsers { get; set; }
        public int WaitingUsersCount { get; set; }
        public int PairedUsersCount { get; set; }
    }
}
