import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})

export class SignalRService {
  private configService = inject(ConfigService);
  private hubConnection!: signalR.HubConnection;

  startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.configService.baseUrl}/audioHub`)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection Started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  };

  addAudioStatusListener(callback: (fileId: string, status: string, progress: number, errorMessage: string | undefined) => void) {
    this.hubConnection.on('AudioStatusUpdated', (fileId, status, progress, errorMessage) => {
      callback(fileId, status, progress, errorMessage);
    });
  }

  cancelProcessing(fileId: string) {
    this.hubConnection.invoke('CancelProcessing', fileId)
      .then(() => console.log(`Cancel request sent for fileId: ${fileId}`))
      .catch(err => console.error('Error while sending cancel request: ' + err));
  }
}
