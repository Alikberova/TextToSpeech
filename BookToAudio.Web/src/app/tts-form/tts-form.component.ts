import { Component, OnInit, inject} from '@angular/core';
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
  private configService = inject(ConfigService);
  
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

  maxLengthInput = 100000;
  warnedMaxInputLength = false;

  private audio: HTMLAudioElement | null = null;
  isPlaying = false; 
  currentlyLoadingAudio = false;
  currentlyPlayingVoice: string | null = null;
  isPaused = false;

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
    this.validateInputLength();
  }

  playVoiceSample(event: MouseEvent, voice: string, speed: number): void {
    event.stopPropagation();
    if (this.currentlyPlayingVoice === voice && this.audio){
      this.audio.play();
      this.isPaused = false
    }
    else{
    this.currentlyLoadingAudio = true;
    this.currentlyPlayingVoice = voice;
      if (this.audio) {
        this.audio.pause();
        URL.revokeObjectURL(this.audio.currentSrc)
      }
    this.isPaused = false;
    const request: SpeechRequest = {
      model: this.textToSpeech.model,
      voice: SpeechVoice[voice as keyof typeof SpeechVoice],
      speed: speed,
      input: 'Welcome to our voice showcase! Listen as we bring words to life, demonstrating a range of unique and dynamic vocal styles.',
    };
    this.speechClient.getSpeechSample(request).subscribe({
      next: (blob) => this.playAudio(blob),
      error: (err) => {
        this.currentlyPlayingVoice = null;
        this.errorHandler.handleHttpError(err);
      }
    })}
  }

  playAudio(blob: Blob) {
    this.audio = new Audio();
    const url = URL.createObjectURL(blob);
    this.audio.src = url;
    this.audio
    this.audio.load();
    this.audio.oncanplay = () => {
      this.audio!
      .play().then(()=>{
        this.currentlyLoadingAudio = false;
      })
      .catch((error) => console.log('error to in playAudio.', error));
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
  

  stopPropagation(event: MouseEvent){
    event.stopPropagation();
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
    this.isLoading = false;
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
    const fileNameWithoutExtension = this.textToSpeech.file!.name.replace(/\.[^/.]+$/, '');
    const audioDownloadFilename = fileNameWithoutExtension + '.mp3'; // Store this for the download attribute
    const apiUrl = `${this.configService.apiUrl}/audio`;
    this.audioDownloadUrl = `${apiUrl}/downloadmp3/${audioFileId}/${audioDownloadFilename}`;
  }

  validateInputLength(){
    if (this.textToSpeech.file !== undefined && this.textToSpeech.file.size > this.maxLengthInput){
        this.clearFileSelection();
      this.warnedMaxInputLength = true;
      return;
    }
    this.warnedMaxInputLength = false;
  }
}
