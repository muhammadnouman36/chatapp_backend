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
        ResponseVM ContactUs(ContactUsVM model);
        Task<ResponseVM> GetAll(ContactUsFilterVM filter);
        ContactUsVM GetContactById(long id);
        ResponseVM DeleteContact(long id);
    }
}
