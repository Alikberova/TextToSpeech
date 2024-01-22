using Microsoft.AspNetCore.SignalR;

namespace BookToAudio.RealTime;

public class AudioHub : Hub
{
    public async Task NotifyAudioStatus(string fileId, string status)
    {
        await Clients.All.SendAsync("AudioStatusUpdated", fileId, status);
    }
}
