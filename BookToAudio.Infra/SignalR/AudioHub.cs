using BookToAudio.Core.Config;
using Microsoft.AspNetCore.SignalR;

namespace BookToAudio.Infra.SignalR;

public sealed class AudioHub : Hub
{
    public async Task NotifyAudioStatus(Guid fileId, string status)
    {
        await Clients.All.SendAsync(SharedConstants.AudioStatusUpdated, fileId, status);
    }
}
