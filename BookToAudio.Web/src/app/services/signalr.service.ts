import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ConfigConstants } from '../constants/config-constants';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${ConfigConstants.BaseUrl}/audioHub`)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection Started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  };

  addAudioStatusListener(callback: (fileId: string, status: string) => void) {
    this.hubConnection.on('AudioStatusUpdated', (fileId, status) => {
      callback(fileId, status);
    });
  }
}
