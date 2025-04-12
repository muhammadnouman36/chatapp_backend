using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.VMs.AppUsers.Autentication
{
    public class DeclineUserVM
    {
        public long Id { get; set; }
        public string Reason { get; set; } = "";
    }
}
