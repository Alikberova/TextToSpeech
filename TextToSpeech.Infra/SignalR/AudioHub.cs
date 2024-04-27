using TextToSpeech.Core.Config;
using Microsoft.AspNetCore.SignalR;

namespace TextToSpeech.Infra.SignalR;

public sealed class AudioHub : Hub
{
    public async Task NotifyAudioStatus(Guid fileId, string status)
    {
        await Clients.All.SendAsync(SharedConstants.AudioStatusUpdated, fileId, status);
    }
}
