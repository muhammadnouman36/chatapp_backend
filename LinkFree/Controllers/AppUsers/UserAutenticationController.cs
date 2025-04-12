using Application.Interfaces.AppUsers;
using Application.Interfaces.LinkFree;
using Application.VMs.AppUsers.Autentication;
using Application.VMs.LinkFree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers.AppUsers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAutenticationController : ControllerBase
    {
        private readonly IUserAutentication _services;

        public UserAutenticationController(IUserAutentication services)
        {
            _services = services;
        }

        [HttpPost("create-user")]
        public IActionResult CreateUser(UserVM user)
        {

            var result = _services.CreateUser(user);
            return Ok(result);
        }

        [HttpPost("login-user")]
        public IActionResult LogInUser(LoginUserVM model)
        {

            var result = _services.loginUser(model);
            return Ok(result);
        }

        [HttpPost("ContinueWithGoogle")]
        public async Task<IActionResult> ContinueWithGoogle(string idToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _services.LoginWithGoogle(idToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("ChangeUserPassword")]
        public IActionResult ChangeUserPassword(ChangeUserPassowrdVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.ChangeUserPassword(model);
            return Ok(result);
        }

        [HttpPost("ForgetUserPassword")]
        public IActionResult ForgetUserPassword(string Email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.ForgetUserPassword(Email);

            return Ok(result);
        }

        [HttpPost("ResetUserPassword")]
        public IActionResult ResetUserPassword(string ResetToken, string password)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.ResetUserPassword(ResetToken, password);
            return Ok(result);
        }

        [HttpPost("UnBlockUser")]
        public IActionResult UnBlockUser(long Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.UnBlockUser(Id);
            return Ok(result);
        }
        [HttpPost("BlockUser")]
        public IActionResult BlockUser(DeclineUserVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.BlockUser(model);
            return Ok(result);
        }

        [HttpPost("DeletUser")]
        public IActionResult DeleteUser(long Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _services.DeleteUser(Id);
            return Ok(result);
        }
    }
}
