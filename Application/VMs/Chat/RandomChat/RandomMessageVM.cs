using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.VMs.Chat.RandomChat
{
    public class RandomMessageVM
    {
        public string? Message { get; set; }
        public string? ImageBase64 { get; set; } 
        public DateTime Timestamp { get; set; }
        public bool IsSeen { get; set; }
        public bool IsOnline { get; set; }
        public string MessageType { get; set; }
    }
}
