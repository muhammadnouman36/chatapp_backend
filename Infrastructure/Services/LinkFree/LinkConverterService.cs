using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.LinkFree;
using Application.VMs;
using Application.VMs.LinkFree;
using Azure;
using Azure.Core;
using Domain.Models.LinkFree;
using Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Services.LinkFree
{
    public class LinkConverterService : ILinkConverter
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;


        public LinkConverterService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }
        public ResponseVM InstagramLink(InstagramLinks insta)
        {
            ResponseVM response = ResponseVM.Instance;

            if (string.IsNullOrWhiteSpace(insta.orgLink))
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid Instagram link provided.";
                return response;
            }
            //long id = ExtractIdFromLink(insta.orgLink);
            //var existingLink = _context.Instagram.FirstOrDefault(i => i.OrgLink == insta.orgLink);
            //if (existingLink != null)
            //{
            //    response.responseCode = 200;
            //    response.responseMessage = "Link already exists.";
            //    response.data = existingLink.NewLink;
            //    return response;
            //}

            var newEntry = new Instagram
            {
                OrgLink = insta.orgLink
            };
            _context.Instagram.Add(newEntry);
            _context.SaveChanges();

            newEntry.NewLink = $"http://localhost:7079/api/linktree/getlinks?getid={newEntry.Id}";
            _context.Instagram.Update(newEntry);
            _context.SaveChanges();

            response.responseCode = 200;
            response.responseMessage = "Link added successfully.";
            response.data = newEntry.NewLink;
            return response;
        }


        public string getLink(string link)
        {
            long id = ExtractIdFromLink(link);
            if(id != 0)
            {
                var instaLink = _context.Instagram.FirstOrDefault(i => i.Id ==  id);
                if(instaLink != null)
                {
                    return instaLink.OrgLink;
                }
                else
                {
                   return "No Link with this id";
                }
            }
            else
            {
                return "Invalid Id";
            }
        }





        public static long ExtractIdFromLink(string link)
        {
            string key = "getid=";
            int startIndex = link.IndexOf(key);

            if (startIndex != -1)
            {
                startIndex += key.Length;
                int endIndex = link.IndexOf("&", startIndex);
                string idString = endIndex == -1 ? link.Substring(startIndex) : link.Substring(startIndex, endIndex - startIndex);

                if (long.TryParse(idString, out long id))
                {
                    return id;
                }
            }
            return 0;
        }

    }
}
