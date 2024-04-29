import { Component, OnInit, inject} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SpeechRequest } from '../models/text-to-speech';
import { SpeechClient } from '../http-clients/speech-client';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { SignalRService } from '../services/signalr.service';
import { ViewChild, ElementRef } from '@angular/core';
import { SnackbarService } from '../shared-ui/snackbar-service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormatMaxInputLengthPipe } from '../pipe/format-max-input';
import { ConfigService } from '../services/config-service';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterOutlet, MatTooltipModule, MatProgressBarModule, MatButtonModule, MatIconModule, MatSelectModule, MatInputModule, FormatMaxInputLengthPipe],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss',
})

export class TtsFormComponent implements OnInit {
  
  constructor(
    private speechClient: SpeechClient,
    private signalRService: SignalRService,
    private snackBarService: SnackbarService,
    private configService: ConfigService
  ) {}

  ngOnInit(): void {
    this.signalRService.startConnection();
    this.signalRService.addAudioStatusListener(this.handleAudioStatusUpdate.bind(this));
  }

  readonly voices = [ 'Alloy', 'Echo', 'Fable', 'Onyx', 'Nova', 'Shimmer' ];
  readonly models = ['tts-1', 'tts-1-hd'];
  readonly ttsApis = ['OpenAI', 'Narakeet'];
  readonly acceptableFileTypes = ['.pdf', '.txt'];
  readonly maxInputLength = 100000;

  uploadedFile: File | undefined;
  voiceSpeed = 1;
  selectedVoice = this.voices[0];

  @ViewChild('fileInput') fileInput!: ElementRef;
  isTextConversionLoading = false;
  isSpeechReady = false;
  audioDownloadUrl = '';
  warnedMaxInputLength = false;
  isPlaying = false; 
  isSampleLoading = false;
  currentlyPlayingVoice: string | null = null;
  isPaused = false;
  private audio: HTMLAudioElement | null = null;

  onFileSelected(event: Event) {
    this.warnedMaxInputLength = false;
    const target = event.target as HTMLInputElement;
    if (!target.files || target.files.length == 0) {
      return;
    }
    if (!this.acceptableFileTypes.some(type => target.files![0].name.endsWith(type))) {
      this.isSpeechReady = false;
      this.snackBarService.showError("Oops! 🙈 Looks like I can't work my magic on this file type. Stick with PDFs or text files for the best results, okay? 🚀");
      return;
    }
    if (target.files[0].size > this.maxInputLength) {
      this.clearFileSelection();
      this.warnedMaxInputLength = true;
      this.isSpeechReady = false;
      return;
    }
    this.uploadedFile = target.files[0];
  }

  playVoiceSample(event: MouseEvent, voice: string, speed: number): void {
    event.stopPropagation();
    if (this.currentlyPlayingVoice === voice && this.audio){
      this.audio.play();
      this.isPaused = false
      return;
    }
    this.isSampleLoading = true;
    this.currentlyPlayingVoice = voice;
    if (this.audio) {
      this.audio.pause();
      URL.revokeObjectURL(this.audio.currentSrc)
    }
    this.isPaused = false;
    const request: SpeechRequest = {
      ttsApi: this.ttsApis[0],
      model: this.models[0],
      voice: voice,
      speed: speed,
      languageCode: 'en-US',
      input: 'Welcome to our voice showcase! Listen as we bring words to life, demonstrating a range of unique and dynamic vocal styles!',
    };
    this.speechClient.getSpeechSample(request).subscribe({
      next: (blob) => this.playAudio(blob),
      error: () => {
        this.currentlyPlayingVoice = null;
        this.isSampleLoading = false;
      }
    })
  }

  playAudio(blob: Blob) {
    this.audio = new Audio();
    this.audio.src = URL.createObjectURL(blob);
    this.audio.load();
    this.audio.oncanplay = () => {
      this.audio!.play().then(()=>{
        this.isSampleLoading = false;
      });
    }
    
    this.audio.onended = () => {
      this.isPlaying = false;
      this.currentlyPlayingVoice = null;
    }
  }

  pauseAudio(event: MouseEvent) {
    event.stopPropagation();
    this.audio!.pause();
    this.isPaused = true;
    this.isPlaying = false;
  }

  clearFileSelection() {
    this.uploadedFile = null!;
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onSubmit() {
    this.isTextConversionLoading = true;

    const speechRequest: SpeechRequest = {
      ttsApi: this.ttsApis[0],
      model: this.models[0],
      voice: this.selectedVoice,
      speed: this.voiceSpeed,
      languageCode: 'en-US',
      file: this.uploadedFile
    };

    this.speechClient.createSpeech(speechRequest).subscribe({
      error: () => {
        this.isTextConversionLoading = false;
      }
    });
  }

  private handleAudioStatusUpdate(fileId: string, status: string) {
    this.isTextConversionLoading = false;
      if (status !== 'Completed') {
        this.snackBarService.showError("Oopsie-daisy! Our talking robot hit a snag creating your speech. Let's try again!");
      return;
    }
    this.isSpeechReady = true;
    this.setDownloadData(fileId);
    this.clearFileSelection();
    this.snackBarService.showSuccess('The audio file is ready, you can download it');
  }

  private setDownloadData(audioFileId: string) {
    const fileNameWithoutExtension = this.uploadedFile!.name.replace(/\.[^/.]+$/, '');
    const audioDownloadFilename = fileNameWithoutExtension + '.mp3'; // Store this for the download attribute
    const apiUrl = `${this.configService.apiUrl}/audio`;
    this.audioDownloadUrl = `${apiUrl}/downloadmp3/${audioFileId}/${audioDownloadFilename}`;
  }
}
