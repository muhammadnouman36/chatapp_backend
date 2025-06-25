using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.VMs;

namespace Application.Interfaces.Chat
{
    public interface IChat
    {

        public ResponseVM GetAllUsers();

        public ResponseVM AddFriend(long friendId);

        public ResponseVM GetFriends();
    }
}
