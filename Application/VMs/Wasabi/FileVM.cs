using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.VMs.Wasabi
{
    public class FileVM
    {
        public IFormFile File { get; set; }
        public bool access { get; set; } = false;

    }
}
