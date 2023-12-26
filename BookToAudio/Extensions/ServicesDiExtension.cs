using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infa.Services;
using BookToAudio.Infra.Repositories;
using BookToAudio.Services;

namespace BookToAudio.Extensions;

public static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserService>();
        services.AddScoped<BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
