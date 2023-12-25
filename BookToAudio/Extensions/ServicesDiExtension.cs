using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services;
using BookToAudio.Infra.Repositories;

namespace BookToAudio.Extensions;

public static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<BtaUserManager>();

        return services;
    }
}
