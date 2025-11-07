import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { SERVER_URL } from '../../constants/tokens';
/** Wrapper around SignalR hub for audio generation status updates. */
@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hub?: signalR.HubConnection;

  startConnection() {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) return;
    const audioHubUrl = `${inject(SERVER_URL)}/audiohub`
    this.hub = new signalR.HubConnectionBuilder().withUrl(audioHubUrl).build();
    void this.hub.start().catch((err: unknown) => console.error('SignalR start error', err));
  }

  stopConnection() {
    if (this.hub) {
      void this.hub.stop().catch((err: unknown) => console.error('SignalR stop error', err));
      this.hub = undefined;
    }
  }

  addAudioStatusListener(callback: (fileId: string, status: string, progress: number | null, errorMessage?: string) => void) {
    this.hub?.on('AudioStatusUpdated', (fileId: string, status: string, progress?: number | null, errorMessage?: string) => {
      callback(fileId, status, progress ?? null, errorMessage);
    });
  }

  cancelProcessing(fileId: string) {
    void this.hub?.invoke('CancelProcessing', fileId).catch((err: unknown) => console.error('SignalR invoke error', err));
  }
}
