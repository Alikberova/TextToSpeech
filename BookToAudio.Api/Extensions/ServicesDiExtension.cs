using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Core.Services.Interfaces.Ai;
using BookToAudio.Infra.Services;
using BookToAudio.Infra.Repositories;
using BookToAudio.Infra.Services.Interfaces;
using BookToAudio.Infra.Services.FileProcessing;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.Common;
using BookToAudio.Api.Services;
using BookToAudio.Infra.Services.Ai.Narakeet;

namespace BookToAudio.Api.Extensions;

internal static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBtaUserManager, BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITtsService, OpenAiService>();
        services.AddScoped<ITtsService, NarakeetService>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<IAudioFileRepositoryService, AudioFileRepositoryService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<IMetaDataService, MetaDataService>();
        services.AddScoped<ITtsServiceFactory, TtsServiceFactory>();

        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IAudioFileService, AudioFileService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<FileProcessorFactory>();
        services.AddSingleton<IFileProcessor, TextFileProcessor>();

        return services;
    }
}
