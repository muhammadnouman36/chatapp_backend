using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Infrastructure.Services.AppUsers;
using Infrastructure.Services.Chat;

namespace LinkFree.InjectedServices
{
    public static class CustomeServices
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUserAutentication, AutenticationUserService>();
            services.AddScoped<IChat, ChatService>();
        }
    }
}
