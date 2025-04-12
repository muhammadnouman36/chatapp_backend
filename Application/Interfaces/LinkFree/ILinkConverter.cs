using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.VMs;
using Application.VMs.LinkFree;

namespace Application.Interfaces.LinkFree
{
    public interface ILinkConverter
    {
        public ResponseVM InstagramLink(InstagramLinks insta);

        public string getLink(string link);
    }
}
