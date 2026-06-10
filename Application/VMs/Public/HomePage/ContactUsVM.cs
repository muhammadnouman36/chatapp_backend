using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.VMs.Public.HomePage
{
    public class ContactUsVM
    {
        public long id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
