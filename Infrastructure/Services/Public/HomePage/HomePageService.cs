using Application.Constants.Public.HomePage;
using Application.Interfaces.Public.HomePage;
using Application.VMs;
using Application.VMs.Public.HomePage;
using Domain.Public.HomePage;
using Infrastructure.Context;
using Infrastructure.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Public.HomePage
{
    public class HomePageService : IHomePage
    {
        public AppDbContext _context;
        public HomePageService(AppDbContext context)
        {
            _context = context;
        }
        public ResponseVM ContactUs(ContactUsVM model)
        {
            ResponseVM response = ResponseVM.Instance;

            if(model.id == 0)
            {
                ContactUs contactUs = new ContactUs
                {
                    Name = model.Name,
                    Email = model.Email,
                    Subject = model.Subject,
                    Message = model.Message,
                    Status = ContactStatus.Received
                };
                response.responseCode = 200;
                response.responseMessage = "Thank you for reaching out! We have received your message and will contact you soon.";
                _context.ContactUs.Add(contactUs);
                _context.SaveChanges();
            }
            else
            {
                ContactUs contact = _context.ContactUs.FirstOrDefault(x => x.Id == model.id);
                if (contact == null)
                {
                    response.responseCode = 404;
                    response.responseMessage = "Contact request not found.";
                    return response;
                }

                contact.Status = model.Status;
                response.responseCode = 200;
                response.responseMessage = "Status updated successfully.";


                _context.ContactUs.Update(contact);
                _context.SaveChanges();
            }
           
          
            return response;
        }


        public List<ContactUsVM> GetAllContacts()
        {
            return _context.ContactUs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ContactUsVM
                {
                    id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Subject = x.Subject,
                    Message = x.Message,
                    Status = x.Status
                })
                .ToList();
        }


        public ContactUsVM GetContactById(long id)
        {
            return _context.ContactUs
                .Where(x => x.Id == id)
                .Select(x => new ContactUsVM
                {
                    id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Subject = x.Subject,
                    Message = x.Message,
                    Status = x.Status
                })
                .FirstOrDefault();
        }


        public ResponseVM DeleteContact(long id)
        {
            ResponseVM response = ResponseVM.Instance;

            ContactUs contact = _context.ContactUs
                .FirstOrDefault(x => x.Id == id);

            if (contact == null)
            {
                response.responseCode = 404;
                response.responseMessage = "Contact request not found.";
                return response;
            }

            _context.ContactUs.Remove(contact);
            _context.SaveChanges();

            response.responseCode = 200;
            response.responseMessage = "Contact request deleted successfully.";

            return response;
        }


    }
}
