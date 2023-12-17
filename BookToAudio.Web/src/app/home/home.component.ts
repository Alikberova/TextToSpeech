import { Component } from '@angular/core';
import { FileHadlingService } from '../services/file-hadling.service';
import { TtsClientService as TtsClient } from '../clients/tts-client.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  constructor(private fileHandling: FileHadlingService,
    private ttsClient: TtsClient) {
  }

  onFileSelected(event: any): void {
    this.fileHandling.onFileSelected(event);
  }

  uploadBook(): void {
    this.fileHandling.uploadBook();
  }

  generateAudio(): void {
    this.ttsClient.generateAudio();
  }
}
