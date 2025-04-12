using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Application.VMs.AppUsers.Autentication;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers.Chat
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChatConroller : ControllerBase
    {
        private readonly IChat _services;

        public ChatConroller(IChat services)
        {
            _services = services;
        }


        [HttpGet("Get-all-users")]
        public IActionResult GetAllUsers()
        {

            var result = _services.GetAllUsers();
            return Ok(result);
        }
    }
}
