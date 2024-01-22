import { Component } from '@angular/core';
import { AppMaterialModule } from '../app.material/app.material.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { OpenaiClient } from '../http-clients/openai-client';
import { SpeechRequest } from '../models/text-to-speech';
import { SpeechClient } from '../http-clients/speech-client';
import {MatTooltipModule} from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ErrorHandlerService } from '../services/error-handler-service';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [FormsModule, AppMaterialModule, CommonModule, RouterOutlet, MatTooltipModule, MatProgressBarModule ],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss',
})

export class TtsFormComponent {
  constructor(
    private openAiClient: OpenaiClient,
    private speechClient: SpeechClient,
    private errorHandler: ErrorHandlerService
  ) {}

  voices = ['alloy', 'echo', 'fable', 'onyx', 'nova', 'shimmer'];
  models = ['tts-1', 'tts-1-hd'];
  isLoading = false;
  isSpeechReady = false;

  textToSpeech: SpeechRequest = {
    model: this.models[0],
    voice: this.voices[0],
    speed: 1,
  };

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.textToSpeech.file = target.files[0];
      console.log(this.textToSpeech.file);
    }
  }
  
  //todo change input "now time"
  playVoiceSample(event: MouseEvent, voice: string, speed: number): void {
    event.stopPropagation();
    const testParams: SpeechRequest = {
      model: this.textToSpeech.model,
      voice: voice,
      speed: speed,
      input: 'это мой голос',
    };
    this.playSample(testParams);
  }

  playSample(param: SpeechRequest) {
    this.openAiClient.createSpeech(param).subscribe((blob: Blob) => {
      const audio = new Audio();
      const url = URL.createObjectURL(blob);
      audio.src = url;
      audio.load();
      audio
        .play()
        .catch((error) => console.log('error to in textToSpeech().', error));
    });
  }

  clearFileSelection() {
    this.textToSpeech.file = null!;
  }

  onSubmit() {
    this.isLoading = true;

    this.speechClient.createSpeech(this.textToSpeech).subscribe({
      next: (resp) => {
        console.log('success', resp);
        this.isLoading = false;
        // todo clearFileSelection
        this.isSpeechReady = true;
      },
      error: (error) => {
        console.error(error);
        this.isLoading = false;
        this.errorHandler.handleHttpError(error);
      }
    });
  }

  download() {
   
  }
}
