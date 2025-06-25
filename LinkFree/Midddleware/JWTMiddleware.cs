using Application.VMs.Token;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace LinkFree.Midddleware
{
    public class JWTMiddleware : IMiddleware
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JWTMiddleware(TokenValidationParameters tokenValidationParameters)
        {
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var claimsPrincipal = _jwtSecurityTokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);

                context.User = claimsPrincipal;


                string? email = claimsPrincipal.FindFirst("Email")?.Value;
                string? id = claimsPrincipal.FindFirst("ID")?.Value;
                string? username = claimsPrincipal.FindFirst("UserName")?.Value;
                TokenVm toekn = new TokenVm();
                TokenVm.UserEmail = email;
                TokenVm.UserName = username;
                if (long.TryParse(id, out long userId))
                {
                    TokenVm.UserID = userId;
                }
                context.Items["UserEmail"] = email;
                context.Items["UserID"] = userId;
                context.Items["UserName"] = username;

            }
            await next(context);
        }
    }
}
