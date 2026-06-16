using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommonOperations.Methods
{
    public static class CommonMethods
    {
        #region JWT
        public static String ConvertStringToShah256(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public static string EncrypthePassword(string Password)
        {
            string password = Base64Encode(Password);
            return ConvertStringToShah256(password);
        }
        public static string Base64Encode(string password)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(password);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string GenerateJwtToken(string UserEmail, long UserID, string UserName, string Role, IConfiguration config)
        {
            var authClaims = new List<Claim>
            {
                new Claim("Email", UserEmail),
                new Claim("ID", UserID.ToString()),
                new Claim("UserName", UserName),
                new Claim(ClaimTypes.Role, string.IsNullOrEmpty(Role) ? "users" : Role)
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: config["JWT:ValidIssuer"],
                audience: config["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(30), // Short-lived access token
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token, IConfiguration config)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"])),
                ValidateLifetime = false // Here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        public static string ExtractClaimFromToken(string token, string claimType)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);
                return claim?.Value;
            }
            catch
            {
                return null; 
            }
        }

        #endregion


        #region SMTP
        public static async Task<string> SendEmail(string userEmail, string subject, string body, IConfiguration config)
        {
            string response = "";
            var smtpSettings = config.GetSection("SmtpSettings");

            using (SmtpClient client = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"])))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings["SenderEmail"], smtpSettings["Password"]);
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpSettings["SenderEmail"]);
                mailMessage.To.Add(userEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                try
                {
                    client.EnableSsl = true;
                    client.Send(mailMessage);
                    response = "mail sent successfully";
                }
                catch (Exception ex)
                {
                    response = $"the error is {ex.Message}";
                }
                return response;
            }
        }

        #endregion


        #region Database
        private static readonly string _connectionString = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build()
    .GetConnectionString("ConnectionString");


        public static string GetConnectionString()
        {
            return _connectionString;
        }
        public static async Task<List<dynamic>> ExecuteStoredProcedures(string storedProcedureName, DynamicParameters parameters = null)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var data = (await connection.QueryAsync<dynamic>(
                    storedProcedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                return data;
            }
        }


        #endregion
    }
}
