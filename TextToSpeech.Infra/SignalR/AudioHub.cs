﻿using Microsoft.AspNetCore.SignalR;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.SignalR;

public sealed class AudioHub(ITaskManager _taskManager) : Hub
{
    public async Task CancelProcessing(Guid audioFileId)
    {
        await _taskManager.TryCancelTask(audioFileId);
    }
}
