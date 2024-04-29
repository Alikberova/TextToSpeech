using TextToSpeech.Core.Repositories;
using TextToSpeech.Core.Services;
using TextToSpeech.Core.Services.Interfaces;
using TextToSpeech.Core.Services.Interfaces.Ai;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Repositories;
using TextToSpeech.Infra.Services.Interfaces;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.Services.Factories;
using TextToSpeech.Infra.Services.Common;
using TextToSpeech.Api.Services;
using TextToSpeech.Infra.Services.Ai.Narakeet;
using TextToSpeech.Infra;

namespace TextToSpeech.Api.Extensions;

internal static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthenticationService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBtaUserManager, BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITtsService, OpenAiService>();
        services.AddScoped<INarakeetService, NarakeetService>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<IMetaDataService, MetaDataService>();
        services.AddScoped<ITtsServiceFactory, TtsServiceFactory>();
        services.AddScoped<DbInitializer>();

        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IAudioFileService, AudioFileService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<FileProcessorFactory>();
        services.AddSingleton<IFileProcessor, TextFileProcessor>();
        services.AddSingleton<IRedisCacheProvider>(new RedisCacheProvider(configuration.GetConnectionString("Redis")!));

        return services;
    }
}
