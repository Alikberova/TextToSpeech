import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Subscription } from 'rxjs';
import { EnLanguageCode } from '../../constants/content/language';
import { TtsApis, Narakeet, DemoText } from '../../constants/tts-constants';
import { SpeechClient } from '../../http-clients/speech-client';
import { DropdownConfig } from '../../models/dropdown-config';
import { SpeechRequest } from '../../models/dto/text-to-speech';
import { AudioService } from '../../services/audio.service';
import { ConfigService } from '../../services/config.service';
import { SignalRService } from '../../services/signalr.service';
import { TranslationService } from '../../services/translation.service';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { SnackbarService } from '../../ui-services/snackbar-service';
import { DropdownService } from '../../services/dropdown.service';
import { FileInputService } from '../../services/file-input.service';
import { AcceptableFileTypes } from "../../constants/tts-constants";

@Component({
    selector: 'app-tts-form',
    standalone: true,
    templateUrl: './tts-form.component.html',
    styleUrl: './tts-form.component.scss',
    imports: [FormsModule, CommonModule, RouterOutlet, MatTooltipModule, MatProgressBarModule, MatButtonModule, MatIconModule, MatSelectModule, MatInputModule, DropdownComponent]
})

export class TtsFormComponent implements OnInit {
  
  constructor(
    private speechClient: SpeechClient,
    private signalRService: SignalRService,
    private snackBarService: SnackbarService,
    private configService: ConfigService,
    private dropdownService: DropdownService,
    private audioService: AudioService,
    private translationService: TranslationService,
    private fileInputService: FileInputService
  ) {
    this.dropdownConfigApi = this.dropdownService.getConfig(null,
      0,
      TtsApis.map((api, index) => ({ id: index, optionDescription: api })),
      this.dropdownService.headingApi);
    this.setLangAndVoiceConfig();
  }

  ngOnInit(): void {
    this.signalRService.startConnection();
    this.signalRService.addAudioStatusListener(this.handleAudioStatusUpdate.bind(this));
  }

  readonly acceptableFileTypes = AcceptableFileTypes;
  readonly icons = { 
    downloading: 'downloading',
    playCircle: 'play_circle',
    pause: 'pause'
  };
 
  voiceSpeed = 1;
  currentAudioFileId = '';
  isTextConversionLoading = false;
  isSpeechReady = false;
  audioDownloadUrl = '';
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
    this.dropdownConfigLanguage.selectedIndex = -1; //reset language and voice; -1 for lang as there different default index it's not 0 and not static
    this.setLangAndVoiceConfig();
    this.audioService.stopAudio();
    this.changeClickedVoiceIcon(this.icons.playCircle)
    this.clickedVoiceMatIconClass = '_';
  }

  languageSelectionChanged(id: number) {
    this.dropdownConfigLanguage.selectedIndex = id;
    this.dropdownConfigVoice.selectedIndex = 0; //reset voice
    this.setLangAndVoiceConfig();
    this.audioService.stopAudio();
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
    const target = event.target as HTMLInputElement;
    if (!target.files || target.files.length == 0) {
      return;
    }

    const error = this.fileInputService.validateFile(target.files![0]);
    if (error) {
      this.isSpeechReady = false;
      this.snackBarService.showError(error);
      return;
    }
    this.fileInputService.uploadedFile = target.files[0];
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

  onSubmit() {
    this.isTextConversionLoading = true;
    const api = this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigApi);
    const speechRequest: SpeechRequest = {
      ttsApi: api,
      voice: this.dropdownService.getSelectedValueFromIndex(this.dropdownConfigVoice).toLowerCase(),
      speed: this.voiceSpeed,
      languageCode: this.dropdownService.selectedLanguageCode,
      file: this.fileInputService.uploadedFile
    };

    this.speechClient.createSpeech(speechRequest).subscribe({
      error: _ => this.isTextConversionLoading = false,
      next: id => this.currentAudioFileId = id
    });
  }

  getUploadedFile(): File | undefined {
    return this.fileInputService.uploadedFile;
  }

  clearFileSelection() {
    this.fileInputService.clearFileSelection();
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
    const fileNameWithoutExtension = this.fileInputService.uploadedFile!.name.replace(/\.[^/.]+$/, '');
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
