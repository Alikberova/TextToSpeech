import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
/** Wrapper around SignalR hub for audio generation status updates. */
@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hub?: signalR.HubConnection;

  /**
   * Start connection to the hub. Accepts API base like `https://localhost:7057/api`.
   * The hub runs at `${baseWithoutApi}/audioHub`.
   */
  startConnection(apiBaseUrl: string) {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) return;
    const base = apiBaseUrl.replace(/\/?api\/?$/, '');
    this.hub = new signalR.HubConnectionBuilder().withUrl(`${base}/audioHub`).build();
    void this.hub.start().catch((err: unknown) => console.error('SignalR start error', err));
  }

  stopConnection() {
    if (this.hub) {
      void this.hub.stop().catch((err: unknown) => console.error('SignalR stop error', err));
      this.hub = undefined;
    }
  }

  addAudioStatusListener(cb: (fileId: string, status: string, progress: number | null, errorMessage?: string) => void) {
    this.hub?.on('AudioStatusUpdated', (fileId: string, status: string, progress?: number | null, errorMessage?: string) => {
      cb(fileId, status, progress ?? null, errorMessage);
    });
  }

  cancelProcessing(fileId: string) {
    void this.hub?.invoke('CancelProcessing', fileId).catch((err: unknown) => console.error('SignalR invoke error', err));
  }
}
