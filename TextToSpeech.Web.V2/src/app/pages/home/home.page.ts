import { Component, Signal, ViewChild, ElementRef, OnDestroy, OnInit, computed, signal, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelect, MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NARAKEET_KEY, OPEN_AI_KEY, PROVIDER_MODELS, ProviderKey, PROVIDERS, ACCEPTABLE_FILE_TYPES } from '../../constants/tts-constants';
import { TtsService } from '../../core/http/tts/tts.service';
import { SignalRService } from '../../core/realtime/signalr.service';
import { VOICES_NARAKEET } from '../../core/http/endpoints';
import { NarakeetVoice } from '../../dto/narakeet-voice';
import { SpeechResponseFormat } from '../../dto/tts-request';
import { SampleAudioPlayer } from './home.sample-audio';
import { buildDownloadFilename, getLanguagesFromNarakeetVoices, getVoicesForProvider, mapStatusToIcon } from './home.helpers';
import { AUDIO_STATUS, FieldKey, SAMPLE_STATUS, type AudioStatus, type SampleStatus, type SelectOption } from './home.types';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSliderModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatChipsModule,
    TranslateModule,
  ],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePage implements OnInit, OnDestroy {
  // 1) Injected services (readonly)
  private readonly translate = inject(TranslateService);
  private readonly http = inject(HttpClient);
  private readonly tts = inject(TtsService);
  private readonly snack = inject(MatSnackBar);
  private readonly signalR = inject(SignalRService);

  // 2) View references
  @ViewChild('providerEl') private readonly providerEl?: MatSelect;
  @ViewChild('modelEl') private readonly modelEl?: MatSelect;
  @ViewChild('languageEl') private readonly languageEl?: MatSelect;
  @ViewChild('voiceEl') private readonly voiceEl?: MatSelect;
  @ViewChild('fileInput') private readonly fileInput?: ElementRef<HTMLInputElement>;

  // 3) Constants and options exposed to the template
  readonly providers = PROVIDERS;
  readonly providerModels = PROVIDER_MODELS;
  readonly fileInputId = 'fileInputEl';
  readonly speedInputId = 'speedInputEl';
  readonly acceptableFileTypes = ACCEPTABLE_FILE_TYPES;
  readonly acceptAttr: string = ACCEPTABLE_FILE_TYPES.join(',');
  readonly acceptableTypesText = computed(() => this.acceptableFileTypes.join(', '));
  readonly responseFormats = [
    { key: SpeechResponseFormat.MP3, label: 'MP3' },
    { key: SpeechResponseFormat.Opus, label: 'OPUS' },
    { key: SpeechResponseFormat.AAC, label: 'AAC' },
    { key: SpeechResponseFormat.Flac, label: 'FLAC' },
    { key: SpeechResponseFormat.WAV, label: 'WAV' },
    { key: SpeechResponseFormat.PCM, label: 'PCM' },
  ] as const;

  // 4) Internal reactive state
  private readonly narakeetVoices = signal<NarakeetVoice[]>([]);
  private readonly langChangeTrigger = signal(0);
  // Tracks if the user has modified the sample text so we avoid auto-translating it.
  private readonly isSampleUserEdited = signal<boolean>(false);
  // Stores the last auto-applied sample text so we can detect divergence.
  private readonly lastAutoSampleText = signal<string>('');
  private readonly samplePlayer = new SampleAudioPlayer(
    () => this.stopSample(),
    () => {
      this.sampleError.set('playback');
      this.stopSample();
    }
  );

  private sampleRequestSub?: Subscription;
  // Generation state
  private readonly currentFileId = signal<string | null>(null);

  // Form signals
  provider = signal<ProviderKey | ''>('');
  model = signal<string>('');
  language = signal<string>('');
  voice = signal<string>('');
  speed = signal<number>(1);
  responseFormat = signal<SpeechResponseFormat>(SpeechResponseFormat.MP3);
  file = signal<File | null>(null);
  statusMessage = signal<string>('');
  submitAttempt = signal(false);
  // Marks a failed attempt to play a sample, to highlight missing fields
  sampleAttempt = signal(false);
  fileTouched = signal(false);
  // Sample playback state
  sampleText = signal<string>('');
  // Playback status for the sample audio
  sampleStatus = signal<SampleStatus>(SAMPLE_STATUS.Stopped);
  sampleError = signal<string | null>(null);
  status = signal<AudioStatus>(AUDIO_STATUS.Idle);
  progress = signal<number>(0);
  errorMessage = signal<string | undefined>(undefined);

  // 5) Derived computed values
  // Narakeet languages derived from loaded voices; OpenAI does not need language
  readonly languages = computed<readonly SelectOption[]>(() => {
    if (this.provider() !== NARAKEET_KEY) {
      return [] as const;
    }
    return getLanguagesFromNarakeetVoices(this.narakeetVoices());
  });

  voicesForProvider: Signal<readonly SelectOption[]> = computed(() => {
    const providerKey = this.provider() || undefined;
    const lang = this.language() || undefined;
    return getVoicesForProvider(providerKey, lang, this.narakeetVoices());
  });

  requiresModel: Signal<boolean> = computed(() => {
    const p = this.provider();
    return !!p && Array.isArray(this.providerModels[p]);
  });

  modelsForProvider: Signal<readonly string[]> = computed(() => {
    const p = this.provider();
    const arr = p ? this.providerModels[p] : null;
    return (arr ?? []) as readonly string[];
  });

  formValid: Signal<boolean> = computed(() => {
    const providerOk = !!this.provider();
    const modelOk = this.requiresModel() ? !!this.model() : true;
    const languageOk = this.provider() === NARAKEET_KEY ? !!this.language() : true;
    const voiceOk = !!this.voice();
    const fileOk = !!this.file();
    return providerOk && modelOk && languageOk && voiceOk && fileOk;
  });

  selectedVoiceLabel = computed(() => this.voicesForProvider().find(v => v.key === this.voice())?.label ?? '');
  // Selected provider label for displaying placeholder text in trigger when empty
  selectedProviderLabel = computed(() => this.providers.find(p => p.key === this.provider())?.label ?? '');

  formatSpeed = (v: number | null) => (v == null ? '' : v.toFixed(1));

  // 6) Constructor
  constructor() {
    this.translate.onLangChange.subscribe(() => {
      this.langChangeTrigger.update((v) => v + 1);
      this.applyDefaultSampleIfNotEdited();
    });
    this.signalR.startConnection();
    this.signalR.addAudioStatusListener((fileId, status, progress, errorMessage) => {
      const id = this.currentFileId();
      if (!id || fileId !== id) {
        return;
      }
      this.status.set(status as AudioStatus);
      if (typeof progress === 'number' && Number.isFinite(progress)) {
        this.progress.set(progress);
      }
      this.errorMessage.set(errorMessage);
      console.log(`Progress update for ${fileId}: status=${status}, progress=${progress}, error=${errorMessage}`);
      if (status === AUDIO_STATUS.Failed) {
        this.openErrorSnackbar('home.progress.failed');
      }
    });
  }

  // 7) Lifecycle hooks
  ngOnInit(): void {
    const initial = this.resolveDefaultSampleText();
    this.sampleText.set(initial);
    this.lastAutoSampleText.set(initial);
    this.isSampleUserEdited.set(false);
  }

  ngOnDestroy(): void {
    this.stopSample();
  }

  // 8) Public event handlers and actions (template-facing)
  onProviderChange(provider: string): void {
    const providerKey = (provider === OPEN_AI_KEY || provider === NARAKEET_KEY) ? provider : '';
    this.provider.set(providerKey);
    this.model.set('');
    this.voice.set('');
    this.language.set('');
    if (providerKey && Array.isArray(this.providerModels[providerKey]) && this.providerModels[providerKey].length > 0) {
      this.model.set(this.providerModels[providerKey][0]);
    }
    if (providerKey === NARAKEET_KEY) {
      this.loadNarakeetVoices('Failed to load Narakeet voices');
    }
  }

  onLanguageChange(lang: string): void {
    this.language.set(lang);
    if (this.provider() !== NARAKEET_KEY) {
      return;
    }
    // Refresh the list from backend; voicesForProvider filters by selected language
    this.loadNarakeetVoices('Failed to refresh Narakeet voices');
  }

  onFileSelected(input: HTMLInputElement): void {
    const f = input.files?.[0] ?? null;
    this.file.set(f);
    this.announce(f ? `Selected file ${f.name}` : 'No file selected');
    this.fileTouched.set(true);
  }

  removeFile(): void {
    this.file.set(null);
    this.announce('File removed');
    this.fileTouched.set(true);
  }

  onDrop(ev: DragEvent): void {
    ev.preventDefault();
    const f = ev.dataTransfer?.files?.[0] ?? null;
    if (f) {
      this.file.set(f);
      this.announce(`Selected file ${f.name}`);
    }
    this.fileTouched.set(true);
  }

  onDragOver(ev: DragEvent): void {
    ev.preventDefault();
  }

  openFileDialog(input: HTMLInputElement): void {
    input.click();
  }

  // Updates the sample text from UI input and marks user-edited state accordingly.
  onSampleTextInput(value: string): void {
    this.sampleText.set(value);
    const lastAuto = this.lastAutoSampleText().trim();
    const current = value.trim();
    this.isSampleUserEdited.set(current.length > 0 && current !== lastAuto);
  }

  playOrToggleSample(): void {
    // Toggle between play/pause/resume based on current state
    const state = this.sampleStatus();
    if (state === SAMPLE_STATUS.Playing) {
      this.pauseSample();
      return;
    }
    if (state === SAMPLE_STATUS.Paused) {
      this.resumeSample();
      return;
    }
    this.sampleError.set(null);
    this.cancelSampleRequest();
    const missingField = this.findFirstMissingField({ includeModel: false, includeFile: false });
    if (missingField) {
      // Flag sample attempt for field highlighting (but do not mark upload/file errors)
      this.sampleAttempt.set(true);
      // Focus first missing field for better UX
      this.focusSampleMissing(missingField);
      this.openErrorSnackbar('home.errors.required');
      return;
    }
    const req = {
      ttsApi: this.provider()!,
      languageCode: this.provider() === OPEN_AI_KEY ? '' : this.language()!,
      input: this.sampleText(),
      ttsRequestOptions: {
        model: this.requiresModel() ? this.model() || undefined : undefined,
        speed: this.speed(),
        voice: this.voice()!,
        // Force MP3 for sample
        responseFormat: SpeechResponseFormat.MP3,
      },
    } as const;
    this.sampleRequestSub = this.tts.getSpeechSample(req).subscribe({
      next: (blob: Blob) => {
        this.sampleAttempt.set(false);
        this.samplePlayer.setBlob(blob);
        this.samplePlayer.play()
          .then(() => this.sampleStatus.set(SAMPLE_STATUS.Playing))
          .catch((e) => {
            console.error(e);
            this.sampleError.set('permission');
            this.sampleStatus.set(SAMPLE_STATUS.Stopped);
          });
      },
      error: (e) => {
        console.error('Sample request failed', e);
        this.sampleError.set('request');
        this.openErrorSnackbar('home.voice.sampleError');
      },
      complete: () => { this.sampleRequestSub = undefined; },
    });
  }

  pauseSample(): void {
    this.samplePlayer.pause();
    this.sampleStatus.set(SAMPLE_STATUS.Paused);
  }

  resumeSample(): void {
    this.samplePlayer.resume()
      .then(() => this.sampleStatus.set(SAMPLE_STATUS.Playing))
      .catch((e) => {
        console.error(e);
        this.sampleStatus.set(SAMPLE_STATUS.Stopped);
      });
  }

  stopSample(): void {
    this.cancelSampleRequest();
    this.samplePlayer.stop();
    this.sampleStatus.set(SAMPLE_STATUS.Stopped);
  }

  submit(): void {
    this.submitAttempt.set(true);
    if (!this.formValid()) {
      this.focusFirstInvalid();
      return;
    }
    const req = {
      ttsApi: this.provider()!,
      languageCode: this.provider() === OPEN_AI_KEY ? '' : this.language()!,
      file: this.file()!,
      input: undefined,
      ttsRequestOptions: {
        model: this.requiresModel() ? this.model() || undefined : undefined,
        speed: this.speed(),
        voice: this.voice()!,
        responseFormat: this.responseFormat(),
      },
    } as const;
    this.status.set(AUDIO_STATUS.Created);
    this.tts.createSpeech(req).subscribe({
      next: (id: string) => {
        this.currentFileId.set(id);
      },
      error: (e) => {
        this.status.set(AUDIO_STATUS.Failed);
        console.error(e);
        this.openErrorSnackbar('home.errors.failed');
      },
    });
  }

  clear(formEl?: HTMLFormElement): void {
    // Stop any ongoing sample playback and clear errors
    this.stopSample();
    this.sampleError.set(null);

    this.provider.set('');
    this.model.set('');
    this.language.set('');
    this.voice.set('');
    this.speed.set(1);
    this.responseFormat.set(SpeechResponseFormat.MP3);
    this.file.set(null);
    // Reset sample text back to its default and mark as not user-edited
    const initialSample = this.resolveDefaultSampleText();
    this.sampleText.set(initialSample);
    this.lastAutoSampleText.set(initialSample);
    this.isSampleUserEdited.set(false);
    formEl?.reset();
    this.announce('Form reset');
    this.submitAttempt.set(false);
    this.fileTouched.set(false);
    this.status.set(AUDIO_STATUS.Idle);
    this.progress.set(0);
    this.currentFileId.set(null);
  }

  download(): void {
    const id = this.currentFileId();
    if (!id) {
      return;
    }
    this.tts.downloadById(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const name = buildDownloadFilename(this.file()!.name, this.responseFormat());
        a.download = name;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 0);
      },
      error: () => this.openErrorSnackbar('home.progress.failed'),
    });
  }

  cancel(): void {
    const id = this.currentFileId();
    if (id) {
      this.signalR.cancelProcessing(id);
    }
  }

  // Icon helper for progress header to keep template tidy
  progressIcon(): string {
    return mapStatusToIcon(this.status());
  }

  // 9) Private helpers
  private announce(msg: string) {
    this.statusMessage.set(msg);
    // Clear after a while so subsequent messages are re-announced
    queueMicrotask(() => this.statusMessage.set(''));
  }

  private focusFirstInvalid() {
    queueMicrotask(() => {
      const missingField = this.findFirstMissingField({ includeModel: true, includeFile: true });
      this.focusField(missingField);
    });
  }

  private openErrorSnackbar(messageKey: string): void {
    const message = this.translate.instant(messageKey);
    this.snack.open(message, undefined, { duration: 4000 });
  }

  private resolveDefaultSampleText(): string {
    const key = 'home.sample.defaultText';
    const translated = this.translate.instant(key);
    return translated;
  }

  private applyDefaultSampleIfNotEdited(): void {
    if (this.isSampleUserEdited()) return;
    const next = this.resolveDefaultSampleText();
    this.sampleText.set(next);
    this.lastAutoSampleText.set(next);
  }

  private focusField(field: FieldKey | null | undefined): void {
    switch (field) {
      case 'provider':
        this.providerEl?.focus();
        break;
      case 'model':
        this.modelEl?.focus();
        break;
      case 'language':
        this.languageEl?.focus();
        break;
      case 'voice':
        this.voiceEl?.focus();
        break;
      case 'file':
        this.fileInput?.nativeElement.focus();
        break;
      default:
        break;
    }
  }

  private findFirstMissingField(options: { includeModel: boolean; includeFile: boolean }): FieldKey | null {
    if (!this.provider()) {
      return 'provider';
    }
    if (options.includeModel && this.requiresModel() && !this.model()) {
      return 'model';
    }
    if (this.provider() === NARAKEET_KEY && !this.language()) {
      return 'language';
    }
    if (!this.voice()) {
      return 'voice';
    }
    if (options.includeFile && !this.file()) {
      return 'file';
    }
    return null;
  }

  private focusSampleMissing(field?: FieldKey): void {
    queueMicrotask(() => {
      const missingField = field ?? this.findFirstMissingField({ includeModel: false, includeFile: false });
      this.focusField(missingField);
    });
  }

  private cancelSampleRequest(): void {
    if (this.sampleRequestSub) {
      try {
        this.sampleRequestSub.unsubscribe();
      } catch {
        // no-op
      }
      this.sampleRequestSub = undefined;
    }
  }

  private loadNarakeetVoices(errorMessage: string): void {
    this.http.get<NarakeetVoice[]>(VOICES_NARAKEET).subscribe({
      next: (voices) => this.narakeetVoices.set(voices ?? []),
      error: (error) => {
        console.error(errorMessage, error);
        this.narakeetVoices.set([]);
      },
    });
  }

}
