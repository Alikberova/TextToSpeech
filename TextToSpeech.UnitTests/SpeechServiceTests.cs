using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using static TextToSpeech.Core.Enums;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Models;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Services;
using TextToSpeech.Infra.SignalR;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class SpeechServiceTests
{
    [Fact]
    public async Task UpdateAudioStatus_SendsStatusToHub()
    {
        var fileId = Guid.NewGuid();
        var (service, clientProxy) = CreateSpeechService();

        var method = typeof(SpeechService).GetMethod("UpdateAudioStatus",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        await (Task)method!.Invoke(service, new object?[] { fileId, Status.Processing.ToString(), 25, null, null })!;

        clientProxy.Verify(c => c.SendCoreAsync(
                Shared.AudioStatusUpdated,
                It.Is<object?[]>(args =>
                    (string?)args[0] == fileId.ToString()
                    && (string?)args[1] == Status.Processing.ToString()
                    && (int?)args[2] == 25
                    && args[3] == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAndProgress_OnlyEmitsWhenProgressIncreases()
    {
        var fileId = Guid.NewGuid();
        var (service, clientProxy) = CreateSpeechService();
        var method = typeof(SpeechService).GetMethod("UpdateStatusAndProgress",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        await (Task)method!.Invoke(service,
            new object?[] { fileId, new ProgressReport { FileId = fileId, ProgressPercentage = 10 }, Status.Processing })!;

        await (Task)method.Invoke(service,
            new object?[] { fileId, new ProgressReport { FileId = fileId, ProgressPercentage = 10 }, Status.Processing })!;

        await (Task)method.Invoke(service,
            new object?[] { fileId, new ProgressReport { FileId = fileId, ProgressPercentage = 20 }, Status.Processing })!;

        var invocations = clientProxy.Invocations
            .Where(i => i.Method.Name == nameof(IClientProxy.SendCoreAsync))
            .Select(inv => (object?[])inv.Arguments[1])
            .ToList();

        Assert.Equal(2, invocations.Count);
        Assert.Equal(10, invocations[0][2]);
        Assert.Equal(20, invocations[1][2]);
    }

    private static (SpeechService service, Mock<IClientProxy> clientProxy) CreateSpeechService()
    {
        var textProcessingService = Mock.Of<ITextProcessingService>();
        var ttsServiceFactory = Mock.Of<ITtsServiceFactory>();
        var pathService = Mock.Of<IPathService>();
        var fileProcessorFactory = Mock.Of<IFileProcessorFactory>();
        var logger = Mock.Of<ILogger<SpeechService>>();
        var metaDataService = Mock.Of<IMetaDataService>();
        var audioFileRepository = Mock.Of<IAudioFileRepository>();
        var taskManager = Mock.Of<ITaskManager>();
        var backgroundTaskQueue = Mock.Of<IBackgroundTaskQueue>();
        var serviceScopeFactory = Mock.Of<IServiceScopeFactory>();
        var redisCacheProvider = Mock.Of<IRedisCacheProvider>();
        var hubClients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clientProxy.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        hubClients.SetupGet(c => c.All).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<AudioHub>>();
        hubContext.SetupGet(h => h.Clients).Returns(hubClients.Object);

        var service = new SpeechService(textProcessingService,
            ttsServiceFactory,
            pathService,
            fileProcessorFactory,
            hubContext.Object,
            logger,
            metaDataService,
            audioFileRepository,
            taskManager,
            backgroundTaskQueue,
            serviceScopeFactory,
            redisCacheProvider);

        return (service, clientProxy);
    }
}
