using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Core.Services.Interfaces.Ai;
using BookToAudio.Infra.Services;
using BookToAudio.Infra.Repositories;
using BookToAudio.Services;

namespace BookToAudio.Extensions;

public static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationService>();
        services.AddScoped<SpeechService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITextFileService, TextFileService>();
        services.AddScoped<IOpenAiService, OpenAiService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IAudioFileService, AudioFileService>();
        services.AddScoped<IPathService, PathService>();

        return services;
    }
}
