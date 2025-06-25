using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Application.VMs.AppUsers.Autentication;
using Microsoft.AspNetCore.Authorization;
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
            if (!ModelState.IsValid)
                return BadRequest();
            var result = _services.GetAllUsers();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("Get-friends")]
        public IActionResult GetFriends()
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var result = _services.GetFriends();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("Add-friend")]
        public IActionResult AddFriend(long friendId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var result = _services.AddFriend(friendId);
            return Ok(result);
        }
    }
}
