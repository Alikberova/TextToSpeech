import { Component, OnInit } from '@angular/core';
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
import { DropdownComponent } from "../shared-ui/components/dropdown/dropdown.component";
import { DropdownConfig } from '../models/dropdown-config';
import { DropdownConfigService as DropdownService } from '../services/dropdown.service';
import { AudioService } from '../services/audio.service';
import { TranslationService } from '../services/translation.service';
import { Subscription } from 'rxjs';
import { TtsApis, DemoText, Narakeet } from '../constants/tts-constants';
import { EnLanguageCode } from '../constants/language';

@Component({
    selector: 'app-tts-form',
    standalone: true,
    templateUrl: './tts-form.component.html',
    styleUrl: './tts-form.component.scss',
    imports: [FormsModule, CommonModule, RouterOutlet, MatTooltipModule, MatProgressBarModule, MatButtonModule, MatIconModule, MatSelectModule, MatInputModule, FormatMaxInputLengthPipe, DropdownComponent]
})

export class TtsFormComponent implements OnInit {
  
  constructor(
    private speechClient: SpeechClient,
    private signalRService: SignalRService,
    private snackBarService: SnackbarService,
    private configService: ConfigService,
    private dropdownService: DropdownService,
    private audioService: AudioService,
    private translationService: TranslationService
  ) {
    this.dropdownConfigApi = this.dropdownService.getConfig(null,
      this.dropdownService.defaultSelectedLanguageIndex,
      TtsApis.map((api, index) => ({ id: index, optionDescription: api })),
      this.dropdownService.headingApi);
    this.setLangAndVoiceConfig();
  }

  ngOnInit(): void {
    this.signalRService.startConnection();
    this.signalRService.addAudioStatusListener(this.handleAudioStatusUpdate.bind(this));
  }
  
  readonly acceptableFileTypes = ['.pdf', '.txt'];
  readonly maxInputLength = 100000;
  readonly icons = { 
    downloading: 'downloading',
    playCircle: 'play_circle',
    pause: 'pause'
  };

  uploadedFile: File | undefined;
  voiceSpeed = 1;
  currentAudioFileId = '';
  @ViewChild('fileInput') fileInput!: ElementRef;
  isTextConversionLoading = false;
  isSpeechReady = false;
  audioDownloadUrl = '';
  warnedMaxInputLength = false;
  clickedMatIcon: string | undefined;
  clickedVoiceMatIconClass: string | undefined;
  dropdownConfigApi!: DropdownConfig;
  dropdownConfigLanguage!: DropdownConfig;
  dropdownConfigVoice!: DropdownConfig;

  private currentlyPlayingVoice: string | null = null;
  private currentlyPlayingSpeed: number | null = null;
  private currentlyPlayingLanguageCode: string | null = null;
  private speechSampleSubscription : Subscription | null = null;

  apiSelectionChanged(id: number) {
    this.dropdownConfigApi = this.dropdownService.getConfig(this.dropdownConfigApi, id ?? 0);
    this.dropdownConfigLanguage.selectedIndex = 0; //reset language and voice
    this.setLangAndVoiceConfig();
    this.audioService.revokeAudioSample();
    this.changeClickedVoiceIcon(this.icons.playCircle)
    this.clickedVoiceMatIconClass = '_';
  }

  languageSelectionChanged(id: number) {
    this.dropdownConfigLanguage.selectedIndex = id;
    this.dropdownConfigVoice.selectedIndex = 0; //reset voice
    this.setLangAndVoiceConfig();
    this.audioService.revokeAudioSample();
    this.changeClickedVoiceIcon(this.icons.playCircle)
    this.clickedVoiceMatIconClass = '_';
  }

  voiceSelectionChanged(id: number) {
    this.dropdownConfigVoice.selectedIndex = id;
    this.setLangAndVoiceConfig();
  }

  setLangAndVoiceConfig() {
    const api = this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigApi);
    if (api.includes(Narakeet)) {
      this.dropdownService.getLangAndVoiceConfigForNarakeet(this.dropdownConfigLanguage, this.dropdownConfigVoice)
        .subscribe(({ languageConfig, voiceConfig }) => {
          this.dropdownConfigLanguage = languageConfig;
          this.dropdownConfigVoice = voiceConfig;
      });
      return;
    }
    const result = this.dropdownService.getLangAndVoiceConfigForOpenAi(this.dropdownConfigLanguage, this.dropdownConfigVoice);
    this.dropdownConfigLanguage = result.languageConfigRes;
    this.dropdownConfigVoice = result.voiceConfigRes;
  }

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

  playVoiceSample(event: MouseEvent, index: number): void {
    event.stopPropagation();
    const api = this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigApi);
    const languageCode = this.dropdownService.selectedLanguageCode;
    const voice = this.dropdownConfigVoice.dropDownList[index].optionDescription.toLowerCase();
    if (this.audioService.isPlaying()) {
      this.audioService.pauseAudio();
      this.changeClickedVoiceIcon(this.icons.playCircle)
      if (this.isCurrentAudioTheSameAsPrevious(voice, this.voiceSpeed, languageCode)) {
        return;
      }
    }
    if (this.isCurrentAudioTheSameAsPrevious(voice, this.voiceSpeed, languageCode)) {
      this.audioService.play()
      this.changeClickedVoiceIcon(this.icons.pause)
      return;
    }
    if (this.speechSampleSubscription) {
      this.speechSampleSubscription.unsubscribe();
    }
    this.changeClickedVoiceIcon(this.icons.downloading)
    this.setCurrentlyPlayingData(voice, this.voiceSpeed, languageCode);
    this.sendRequestAndPlaySample(api, voice, this.voiceSpeed, languageCode);
  }

  clearFileSelection() {
    this.uploadedFile = null!;
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onSubmit() {
    this.isTextConversionLoading = true;
    const api = this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigApi);
    const speechRequest: SpeechRequest = {
      ttsApi: api,
      voice: this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigVoice).toLowerCase(),
      speed: this.voiceSpeed,
      languageCode: this.dropdownService.selectedLanguageCode,
      file: this.uploadedFile
    };

    this.speechClient.createSpeech(speechRequest).subscribe({
      error: _ => this.isTextConversionLoading = false,
      next: id => this.currentAudioFileId = id
    });
  }

  private sendRequestAndPlaySample(api: string, voice: string, speed: number, langCode: string) {
    const request: SpeechRequest = {
      ttsApi: api,
      voice: voice,
      speed: speed,
      languageCode: langCode,
      input: DemoText,
    };
    const shortLangCode = langCode.split('-')[0];
    if (shortLangCode == EnLanguageCode) {
      this.speechSampleSubscription = this.sendSampleRequest(request);
      return;
    }
    this.translationService.translateFromEnglish(shortLangCode,
      request.input!,
      (translation) => {
        request.input = translation;
        this.speechSampleSubscription = this.sendSampleRequest(request);
      }
    );
  }

  private sendSampleRequest(request: SpeechRequest) {
    return this.speechClient.getSpeechSample(request).subscribe({
      next: (blob) => {
        this.audioService.playAudio(blob,
          () => this.changeClickedVoiceIcon(this.icons.pause),
          () => this.changeClickedVoiceIcon(this.icons.playCircle))
      },
      error: () => {
        this.setCurrentlyPlayingData(null, null, null);
        this.changeClickedVoiceIcon(this.icons.playCircle)
      }
    })
  }

  private changeClickedVoiceIcon(icon: string) {
    this.clickedMatIcon = icon;
    this.clickedVoiceMatIconClass = DropdownComponent.activeMatIconClass;
  }

  private handleAudioStatusUpdate(fileId: string, status: string, errorMessage: string | undefined) {
    if (this.currentAudioFileId !== fileId) {
      return;
    }
    this.isTextConversionLoading = false;
    if (status !== 'Completed') {
      const langMismatchError = "Select a different voice matching the language of your text";
      const error = errorMessage && errorMessage.includes(langMismatchError)
        ? "Please, " + langMismatchError.charAt(0).toLowerCase() + langMismatchError.slice(1)
        : "Oopsie-daisy! Our talking robot hit a snag creating your speech. Let's try again!"
      this.snackBarService.showError(error);
    return;
    }
    this.isSpeechReady = true;
    this.setDownloadData(fileId);
    this.clearFileSelection();
    this.snackBarService.showSuccess('The audio file is ready, you can download it');
  }

  private setDownloadData(audioFileId: string) { //todo move
    const fileNameWithoutExtension = this.uploadedFile!.name.replace(/\.[^/.]+$/, '');
    const audioDownloadFilename = fileNameWithoutExtension + '.mp3'; // Store this for the download attribute
    const apiUrl = `${this.configService.apiUrl}/audio`;
    this.audioDownloadUrl = `${apiUrl}/downloadmp3/${audioFileId}/${audioDownloadFilename}`;
  }

  private setCurrentlyPlayingData(voice: string | null, speed: number | null, languageCode: string | null) {
    this.currentlyPlayingVoice = voice;
    this.currentlyPlayingSpeed = speed;
    this.currentlyPlayingLanguageCode = languageCode;
  }

  private isCurrentAudioTheSameAsPrevious(voice: string, speed: number, languageCode: string) {
    return this.currentlyPlayingVoice === voice
      && this.currentlyPlayingSpeed === speed
      && this.currentlyPlayingLanguageCode === languageCode;
  }
}
