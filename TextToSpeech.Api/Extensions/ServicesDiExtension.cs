using TextToSpeech.Core.Services;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Repositories;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.Services.Common;
using TextToSpeech.Api.Services;
using TextToSpeech.Infra.Services.Ai;
using TextToSpeech.Infra;
using Microsoft.Extensions.Options;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Api.Extensions;

internal static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthenticationService>();
        services.AddScoped<IBtaUserManager, BtaUserManager>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITtsService, OpenAiService>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<IMetaDataService, MetaDataService>();
        services.AddScoped<ITtsServiceFactory, TtsServiceFactory>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<ITranslationClientWrapper, TranslationClientWrapper>();

        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<IFileProcessorFactory, FileProcessorFactory>();
        services.AddSingleton<IFileProcessor, TextFileProcessor>();
        services.AddSingleton<IFileProcessor, PdfProcessor>();
        services.AddSingleton<IRedisCacheProvider>(new RedisCacheProvider(configuration.GetConnectionString("Redis")!));

        AddServicesWithHttpClient(services);

        return services;
    }

    private static void AddServicesWithHttpClient(IServiceCollection services)
    {
        services.AddHttpClient<INarakeetService, NarakeetService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<NarakeetConfig>>().Value;

            client.BaseAddress = new Uri(options.ApiUrl);
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        });
    }
}
