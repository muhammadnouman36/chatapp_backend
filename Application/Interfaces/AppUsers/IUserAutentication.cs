using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.VMs;
using Application.VMs.AppUsers.Autentication;

namespace Application.Interfaces.AppUsers
{
    public interface IUserAutentication
    {
        ResponseVM CreateUser(UserVM user);
        ResponseVM loginUser(LoginUserVM model);
        Task<ResponseVM> LoginWithGoogle(string idToken);
        ResponseVM ChangeUserPassword(ChangeUserPassowrdVM model);
        ResponseVM ResetUserPassword(string resetToken, string password);
        ResponseVM ForgetUserPassword(string email);
        ResponseVM UnBlockUser(long Id);
        ResponseVM DeleteUser(long Id);
        ResponseVM BlockUser(DeclineUserVM model);
        ResponseVM RefreshToken(TokenApiModel tokenApiModel);
    }
}
