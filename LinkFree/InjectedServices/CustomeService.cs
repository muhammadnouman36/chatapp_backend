using Application.Interfaces.AppUsers;
using Application.Interfaces.Chat;
using Application.Interfaces.Public.HomePage;
using Infrastructure.Services.AppUsers;
using Infrastructure.Services.Chat;
using Infrastructure.Services.Public.HomePage;

namespace LinkFree.InjectedServices
{
    public static class CustomeService
    {
        public static void AddCustomeService(this IServiceCollection services)
        {
            services.AddScoped<IUserAutentication, AutenticationUserService>();
            services.AddScoped<IChat, ChatService>();
            services.AddSignalR();

            // Public
            //    HomePage

            services.AddScoped<IHomePage, HomePageService>();
        }
    }
}
