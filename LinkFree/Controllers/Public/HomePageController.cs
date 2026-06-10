using Application.Interfaces.Public.HomePage;
using Application.VMs.Public.HomePage;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageController : ControllerBase
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

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll(ContactUsFilterVM filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _service.GetAll(filter);
            return Ok(result);
        }

        [HttpPost("GetContactById")]
        public IActionResult GetContactById(long id)
        {
            var result = _service.GetContactById(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("DeleteContact")]
        public IActionResult DeleteContact(long id)
        {
            var result = _service.DeleteContact(id);
            return Ok(result);
        }
    }

}
