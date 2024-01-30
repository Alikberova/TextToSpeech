import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SpeechRequest } from '../models/text-to-speech';
import { SpeechClient } from '../http-clients/speech-client';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ErrorHandlerService } from '../services/error-handler-service';
import { SignalRService } from '../services/signalr.service';
import { SpeechVoice } from '../models/speech-voice.enum';
import { ConfigConstants } from '../constants/config-constants';
import { ViewChild, ElementRef } from '@angular/core';
import { SnackbarService } from '../shared-ui/snackbar-service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterOutlet, MatTooltipModule, MatProgressBarModule, MatButtonModule, MatIconModule, MatSelectModule, MatInputModule],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss',
})

export class TtsFormComponent implements OnInit {
  constructor(
    private speechClient: SpeechClient,
    private errorHandler: ErrorHandlerService,
    private signalRService: SignalRService,
    private snackBarService: SnackbarService
  ) {}

  ngOnInit(): void {
    this.signalRService.startConnection();
    this.signalRService.addAudioStatusListener(this.handleAudioStatusUpdate.bind(this));
  }

  @ViewChild('fileInput') fileInput!: ElementRef;

  voices = Object.values(SpeechVoice).filter(key => isNaN(Number(key)));
  models = ['tts-1', 'tts-1-hd'];
  isLoading = false;
  isSpeechReady = false;
  audioDownloadUrl = '';

  textToSpeech: SpeechRequest = {
    model: this.models[0],
    voice: SpeechVoice[Object.keys(SpeechVoice)[0] as keyof typeof SpeechVoice],
    speed: 1,
  };

  onFileSelected(event: Event) {
    this.isSpeechReady = false;
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.textToSpeech.file = target.files[0];
    }
  }
  
  playVoiceSample(event: MouseEvent, voice: string, speed: number): void {
    event.stopPropagation();
    const request: SpeechRequest = {
      model: this.textToSpeech.model,
      voice: SpeechVoice[voice as keyof typeof SpeechVoice],
      speed: speed,
      input: 'Welcome to our voice showcase! Listen as we bring words to life, demonstrating a range of unique and dynamic vocal styles.',
    };
    this.speechClient.getSpeechSample(request).subscribe({
      next: (blob) => this.playAudio(blob),
      error: (err) => this.errorHandler.handleHttpError(err)
    })
  }

  playAudio(blob: Blob) {
    const audio = new Audio();
      const url = URL.createObjectURL(blob);
      audio.src = url;
      audio.load();
      audio
        .play()
        .catch((error) => console.log('error to in playAudio.', error));
  }

  clearFileSelection() {
    this.textToSpeech.file = null!;
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onSubmit() {
    this.isLoading = true;

    this.speechClient.createSpeech(this.textToSpeech).subscribe({
      error: (error) => {
        console.error(error);
        this.isLoading = false;
        this.errorHandler.handleHttpError(error);
      }
    });
  }

  private handleAudioStatusUpdate(fileId: string, status: string) {
    console.log(`Status of file ${fileId} updated to ${status}`, new Date());
    if (status !== 'Completed') {
      this.snackBarService.showError(`An error occurred while creating speech. Audio file status is ${status}`);
      return;
    }
    this.isLoading = false;
    this.isSpeechReady = true;
    this.setDownloadData(fileId);
    this.clearFileSelection();
    this.snackBarService.showSuccess('The audio file is ready, you can download it');
  }

  private setDownloadData(audioFileId: string) {
    const apiUrl = `${ConfigConstants.BaseApiUrl}/audio`;
    const fileNameWithoutExtension = this.textToSpeech.file!.name.replace(/\.[^/.]+$/, '');
    const audioDownloadFilename = fileNameWithoutExtension + '.mp3'; // Store this for the download attribute
    this.audioDownloadUrl = `${apiUrl}/downloadmp3/${audioFileId}/${audioDownloadFilename}`;
  }
}
