using Application.VMs;
using Application.VMs.Public.HomePage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Public.HomePage
{
    public interface IHomePage
    {
        public ResponseVM ContactUs(ContactUsVM model);
    }
}
