using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Core.Services.Interfaces.Ai;
using BookToAudio.Infra.Services;
using BookToAudio.Infra.Repositories;
using BookToAudio.Services;
using BookToAudio.Infra.Services.Interfaces;
using BookToAudio.Infra.Services.FileProcessing;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.Common;

namespace BookToAudio.Extensions;

public static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // local services
        services.AddScoped<AuthenticationService>();
        services.AddScoped<SpeechService>();

        // other services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOpenAiService, OpenAiService>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<IAudioFileRepositoryService, AudioFileRepositoryService>();

        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IAudioFileService, AudioFileService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<FileProcessorFactory>();
        services.AddSingleton<IFileProcessor, TextFileProcessor>();

        return services;
    }
}
