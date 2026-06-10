using Application.Interfaces.Public.HomePage;
using Application.VMs.Public.HomePage;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageController : Controller
    {
        public readonly IHomePage _service;

        public HomePageController(IHomePage service)
        {
            _service = service;
        }

        [HttpPost("ContactUs")]
        public IActionResult ContactUs(ContactUsVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _service.ContactUs(model);
            return Ok(result);

        }
    }
}
