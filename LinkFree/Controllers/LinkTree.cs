using Application.Interfaces.LinkFree;
using Application.VMs.LinkFree;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkTree : ControllerBase
    {
        private readonly ILinkConverter _services;

        public LinkTree(ILinkConverter services)
        {
            _services = services;
        }

        [HttpPost("saulink")]
        public IActionResult saveAndUpdateLink(InstagramLinks links)
        {

            var result = _services.InstagramLink(links);
            return Ok(result);
        }


        [HttpGet("getlinks")]
        public IActionResult GetLink(string getid)
        {
            string link = _services.getLink(getid);
            return Redirect(link);
        }
    }
}
