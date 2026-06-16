using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.VMs.AppUsers.Autentication;
using Application.VMs;
using Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.AppUsers;
using Azure;
using Domain.AppUsers;
using CommonOperations.Methods;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.AppUsers
{
    public class AutenticationUserService : IUserAutentication
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AutenticationUserService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public ResponseVM CreateUser(UserVM user)
        {
            ResponseVM response = ResponseVM.Instance;

            if (user.Id == 0) 
            {
                var existingUser = _context.AppUser
                    .FirstOrDefault(x => (x.Email == user.Email || x.UserName == user.UserName)  && !x.IsDeleted);

                if (existingUser != null)
                {
                    response.responseCode = 400;
                    response.errorMessage = existingUser.Email == user.Email
                        ? "User Already Exists with this Email"
                        : "Username Already Exists";
                    return response;
                }

                var userModel = new AppUser
                {
                    Password = CommonMethods.EncrypthePassword(user.Password),
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = "users" // Default role
                };

                _context.AppUser.Add(userModel);
                _context.SaveChanges();

                //CommonMethod.SendVerificationEmail(userModel, _config);

                response.responseCode = 200;
                response.responseMessage = "Account created successfully";
                return response;

            }
            else // Update existing user
            {
                response.responseCode = 400;
                response.responseMessage = "Id Already Exsists";
            }

            return response;
        }

        public ResponseVM loginUser(LoginUserVM model)
        {
            ResponseVM response = ResponseVM.Instance;

            var user = _context.AppUser
                .FirstOrDefault(x => (x.Email == model.Mail || x.UserName == model.Mail) && !x.IsDeleted);

            if (user == null)
            {
                response.responseCode = 400;
                response.responseMessage = "There is no user with this Email or Username, or the account is deleted.";
                return response;
            }


            // Check if the user email is verified
            //if (!user.IsEmailVerified)
            //{
            //    response.responseCode = 400;
            //    response.responseMessage = "Please verify your Email first.";
            //    return response;
            //}


            // Check if the user is blocked
            if (user.IsBlocked)
            {
                response.responseCode = 400;
                response.responseMessage = "Account is Blocked";
                return response;
            }

            // Validate the password
            if (user.Password != CommonMethods.EncrypthePassword(model.Password))
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid password.";
                return response;
            }


            var refreshToken = CommonMethods.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token valid for 7 days
            _context.SaveChanges();

            // Prepare the response data on successful login
            var loginData = new onLoggedInVM
            {
                Token = CommonMethods.GenerateJwtToken(user.Email, user.Id, user.UserName, user.Role, _config),
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RefreshToken = refreshToken,
                Role = user.Role
                //ProfileBase64 = CommonMethods.RetrieveBase64Data(user.ProfileImageUrl)
            };

            response.data = loginData;
            response.responseCode = 200;
            response.responseMessage = "Login successful.";

            return response;
        }

        public async Task<ResponseVM> LoginWithGoogle(string idToken)
        {
            ResponseVM response = ResponseVM.Instance;

            var setting = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _config["Google:Client_id"] }
            };

            // Validate token recived from frontend here
            var res = await GoogleJsonWebSignature.ValidateAsync(idToken, setting);
            if (res == null)
            {
                response.responseCode = 200;
                response.responseMessage = "UnVerified by Google";
                return response;
            }

            // get exsisting users
            var user = await _context.AppUser.FirstOrDefaultAsync(u => u.Email == res.Email && !u.IsDeleted && !u.IsBlocked);

            // create new user in db
            if (user == null)
            {
                var newUser = new AppUser
                {
                    FirstName = res.Name,
                    LastName = "",
                    Email = res.Email,
                    Password = "",
                    ProfileImageUrl = res.Picture,
                    IsEmailVerified = true,
                    UserName = res.Email.Split('@')[0],
                    Role = "users",
                    RefreshToken = CommonMethods.GenerateRefreshToken(),
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
                };

                _context.AppUser.Add(newUser);
                await _context.SaveChangesAsync();

                var loginData = new onLoggedInVM
                {
                    Token = CommonMethods.GenerateJwtToken(newUser.Email, newUser.Id, newUser.UserName, newUser.Role, _config),
                    Email = newUser.Email,
                    UserName = newUser.UserName,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    ProfileBase64 = res.Picture,
                    RefreshToken = newUser.RefreshToken,
                    Role = newUser.Role
                };

                response.responseCode = 200;
                response.responseMessage = "Login successful";
                response.data = loginData;
            }
            else
            {
                user.RefreshToken = CommonMethods.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                _context.AppUser.Update(user);
                await _context.SaveChangesAsync();

                // old user here
                var loginData = new onLoggedInVM
                {
                    Token = CommonMethods.GenerateJwtToken(user.Email, user.Id, user.UserName, user.Role, _config),
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileBase64 = res.Picture,
                    RefreshToken = user.RefreshToken,
                    Role = user.Role
                };
                response.responseCode = 200;
                response.responseMessage = "Login successful";
                response.data = loginData;
            }



            return response;
        }

        public ResponseVM ChangeUserPassword(ChangeUserPassowrdVM model)
        {
            ResponseVM response = ResponseVM.Instance;

            // Validate if NewPassword matches ConfirmPassword
            if (model.NewPassword != model.ConfirmPassword)
            {
                response.responseCode = 400;
                response.responseMessage = "New password and confirmation password do not match.";
                return response;
            }

            // Retrieve the user based on the provided UserID
            var user = _context.AppUser.FirstOrDefault(x => x.Id == model.UserID);

            if (user == null)
            {
                response.responseCode = 400;
                response.responseMessage = "User not found.";
                return response;
            }

            // Validate the old password
            if (user.Password != CommonMethods.EncrypthePassword(model.OldPassword))
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid old password.";
                return response;
            }

            // Update the password and save changes
            user.Password = CommonMethods.EncrypthePassword(model.NewPassword);
            _context.SaveChanges();

            response.responseCode = 200;
            response.responseMessage = "Password changed successfully.";
            return response;
        }

        public ResponseVM ResetUserPassword(string resetToken, string password)
        {
            ResponseVM response = ResponseVM.Instance;

            // Extract email from token using the common method
            var email = CommonMethods.ExtractClaimFromToken(resetToken, "Email");
            if (email == null)
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid or expired reset token";
                return response;
            }

            // Find the user by email and check if they exist
            var user = _context.AppUser.FirstOrDefault(x => x.Email == email && !x.IsDeleted);
            if (user == null)
            {
                response.responseCode = 400;
                response.responseMessage = "There is no user with this Email";
                return response;
            }

            // Update the user's password
            user.Password = CommonMethods.EncrypthePassword(password);
            _context.SaveChanges();

            response.responseCode = 200;
            response.responseMessage = "Password updated successfully";
            return response;
        }

        public ResponseVM ForgetUserPassword(string email)
        {
            ResponseVM response = ResponseVM.Instance;

            
            var existingMail = _context.AppUser
                                              .FirstOrDefault(e => e.Email == email && !e.IsDeleted);
            if (existingMail == null)
            {
                response.responseCode = 400;
                response.errorMessage = "No User Found with this Email";
                return response;  
            }

            // Check if the user's account is blocked
            if (existingMail.IsBlocked)
            {
                response.responseCode = 400;
                response.responseMessage = "Please verify your Email first";
                return response; 
            }

            try
            {
                // Generate reset token
                var resetToken = CommonMethods.GenerateJwtToken(existingMail.Email, existingMail.Id, existingMail.UserName,existingMail.Role, _config);

                // Prepare email subject and body
                string subject = "Reset your Password";
                string appLink = "http://localhost:4200/reset-password?resetToken=";
                string body = $"Click this link to Reset Your Password: {appLink +resetToken}";



                // Send email synchronously
                CommonMethods.SendEmail(existingMail.Email, subject, body, _config);

                response.responseCode = 200;
                response.responseMessage = "Password reset Email sent successfully";
            }
            catch (Exception ex)
            {
                response.responseCode = 400;
                response.responseMessage = $"Error sending password reset Email: {ex.Message}";
            }

            return response;
        }

        public ResponseVM UnBlockUser(long Id)
        {
            ResponseVM response = ResponseVM.Instance;

            // Check if the user exists and is not deleted
            var user = _context.AppUser.FirstOrDefault(a => a.Id == Id && !a.IsDeleted);

            if (user == null)
            {
                response.responseCode = 400;
                response.errorMessage = "No active user found";
                return response; 
            }

            // Unblock the user
            user.IsBlocked = false;
                // Update the user synchronously and save changes
                _context.AppUser.Update(user);
                _context.SaveChanges();

                response.responseCode = 200;
                response.responseMessage = "User UnBlocked";

            return response;
        }

        public ResponseVM DeleteUser(long Id)
        {
            ResponseVM response = ResponseVM.Instance;

            // Check if the user exists and is not already deleted
            var user = _context.AppUser.FirstOrDefault(a => a.Id == Id && !a.IsDeleted);

            if (user == null)
            {
                response.responseCode = 400;
                response.errorMessage = "No active user found";
                return response; 
            }

                user.IsDeleted = true;
                _context.AppUser.Update(user);
                _context.SaveChanges();

                response.responseCode = 200;
                response.responseMessage = "User deleted successfully";

            return response;
        }

        public ResponseVM BlockUser(DeclineUserVM model)
        {
            ResponseVM response = ResponseVM.Instance;

            // Fetch user by ID, and check if user exists in one go
            var user = _context.AppUser.FirstOrDefault(a => a.Id == model.Id && !a.IsDeleted);
            if (user == null)
            {
                response.responseCode = 400;
                response.errorMessage = "No active user found with the provided ID.";
                return response;
            }

            // Block the user
            user.IsBlocked = true;
            user.BlockedReason = model.Reason;

            // Update the user and save changes
            _context.AppUser.Update(user);
            _context.SaveChanges();

            response.responseCode =200;
            response.responseMessage = "User successfully blocked.";
            return response;
        }

        public ResponseVM RefreshToken(TokenApiModel tokenApiModel)
        {
            ResponseVM response = ResponseVM.Instance;

            if (tokenApiModel == null || string.IsNullOrEmpty(tokenApiModel.AccessToken) || string.IsNullOrEmpty(tokenApiModel.RefreshToken))
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid client request";
                return response;
            }

            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;

            var principal = CommonMethods.GetPrincipalFromExpiredToken(accessToken, _config);
            if (principal == null)
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid access token or refresh token";
                return response;
            }

            string email = principal.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            if (email == null)
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid access token or refresh token";
                return response;
            }

            var user = _context.AppUser.FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                response.responseCode = 400;
                response.responseMessage = "Invalid access token or refresh token";
                return response;
            }

            var newAccessToken = CommonMethods.GenerateJwtToken(user.Email, user.Id, user.UserName, user.Role, _config);
            var newRefreshToken = CommonMethods.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Extended time for refresh token
            
            _context.SaveChanges();

            response.responseCode = 200;
            response.responseMessage = "Token refreshed successfully";
            response.data = new TokenApiModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return response;
        }

    }
}
