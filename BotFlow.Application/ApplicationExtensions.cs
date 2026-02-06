using Microsoft.Extensions.DependencyInjection;

namespace BotFlow.Application
{
    public static class ApplicationExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Add application services here
            // services.AddScoped<IUserService, UserService>();
            // services.AddAutoMapper(typeof(MappingProfile));
        }
    }
}