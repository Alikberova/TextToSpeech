using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using OpenAI;
using TextToSpeech.Api.Services;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Services;
using TextToSpeech.Infra;
using TextToSpeech.Infra.Config;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Repositories;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.Services.Ai;
using TextToSpeech.Infra.Services.Common;
using TextToSpeech.Infra.Services.FileProcessing;

namespace TextToSpeech.Api.Extensions;

internal static class ServicesDiExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDbInitializer, DbInitializer>();

        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<ISpeechService, SpeechService>();
        services.AddScoped<IMetaDataService, MetaDataService>();
        services.AddScoped<ITtsServiceFactory, TtsServiceFactory>();
        services.AddScoped<ISmtpClient, SmtpClient>();

        services.AddSingleton<ITextProcessingService, TextProcessingService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<IFileProcessorFactory, FileProcessorFactory>();
        services.AddSingleton<IFileProcessor, TextFileProcessor>();
        services.AddSingleton<IFileProcessor, PdfProcessor>();
        services.AddSingleton<IFileProcessor, EpubProcessor>();
        services.AddSingleton<ITaskManager, TaskManager>();
        services.AddSingleton<IProgressTracker, ProgressTracker>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        services.AddHostedService<QueuedHostedService>();

        RegisterServicesBasedOnTestMode(services, configuration);

        services.AddRedis(configuration);

        return services;
    }

    private static void RegisterServicesBasedOnTestMode(IServiceCollection services, IConfiguration configuration)
    {
        if (HostingEnvironment.IsTestMode())
        {
            // used for selenium tests
            services.AddScoped<IEmailService, EmailServiceStub>();
            services.AddScoped<SimulatedTtsService>();
            services.AddScoped<ITtsService>(sp => sp.GetRequiredService<SimulatedTtsService>());
            services.AddScoped<INarakeetService>(sp => sp.GetRequiredService<SimulatedTtsService>());

            return;
        }

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITtsService, OpenAiService>();

        services.AddSingleton(serviceProvider =>
        {
            var apiKey = configuration[ConfigConstants.OpenAiApiKey];
            return new OpenAIClient(apiKey);
        });

        services.AddHttpClient<INarakeetService, NarakeetService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<NarakeetConfig>>().Value;

            client.BaseAddress = new Uri(options.ApiUrl);
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        });
    }
}
