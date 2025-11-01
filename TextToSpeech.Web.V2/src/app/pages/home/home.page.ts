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
import { NARAKEET_KEY, OPEN_AI_KEY, OPEN_AI_VOICES, PROVIDER_MODELS, ProviderKey, PROVIDERS, ACCEPTABLE_FILE_TYPES } from '../../constants/tts-constants';
import { SpeechResponseFormat } from '../../dto/tts-request';
import { NarakeetVoice } from '../../dto/narakeet-voice';
import { TtsService } from '../../core/http/tts.service';
import { SignalRService } from '../../core/realtime/signalr.service';
import { API_BASE_URL } from '../../constants/tokens';

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
  private readonly apiBaseUrl = inject(API_BASE_URL);

  // 2) View references
  @ViewChild('providerEl') private providerEl?: MatSelect;
  @ViewChild('modelEl') private modelEl?: MatSelect;
  @ViewChild('languageEl') private languageEl?: MatSelect;
  @ViewChild('voiceEl') private voiceEl?: MatSelect;
  @ViewChild('fileInput') private fileInput?: ElementRef<HTMLInputElement>;

  // 3) Constants and options exposed to the template
  readonly providers = PROVIDERS;
  readonly providerModels = PROVIDER_MODELS;
  readonly fileInputId = 'fileInputEl';
  readonly speedInputId = 'speedInputEl';
  readonly acceptableFileTypes = ACCEPTABLE_FILE_TYPES;
  readonly acceptAttr: string = ACCEPTABLE_FILE_TYPES.join(',');
  readonly acceptableTypesText = computed(() => this.acceptableFileTypes.join(', '));

  // 4) Internal reactive state
  private readonly narakeetVoices = signal<NarakeetVoice[]>([]);
  private readonly langChangeTrigger = signal(0);
  // Tracks if the user has modified the sample text so we avoid auto-translating it.
  private readonly isSampleUserEdited = signal<boolean>(false);
  // Stores the last auto-applied sample text so we can detect divergence.
  private readonly lastAutoSampleText = signal<string>('');

  // Form signals
  provider = signal<ProviderKey | ''>('');
  model = signal<string | ''>('');
  language = signal<string | ''>('');
  voice = signal<string | ''>('');
  speed = signal<number>(1.0);
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
  sampleStatus = signal<'stopped' | 'playing' | 'paused'>('stopped');
  sampleError = signal<string | null>(null);
  private sampleAudio?: HTMLAudioElement;
  private sampleUrl?: string;
  private sampleRequestSub?: Subscription;

  // Generation state
  private currentFileId = signal<string | null>(null);
  status = signal<AudioStatus>('Idle');
  progress = signal<number>(0);
  errorMessage = signal<string | undefined>(undefined);

  // 5) Derived computed values
  // Narakeet languages derived from loaded voices; OpenAI does not need language
  readonly languages = computed(() => {
    if (this.provider() !== NARAKEET_KEY) {
      return [] as { key: string; label: string }[];
    }
    const map = new Map<string, string>();
    for (const v of this.narakeetVoices()) {
      if (v.languageCode && !map.has(v.languageCode)) {
        map.set(v.languageCode, v.language || v.languageCode);
      }
    }
    return Array.from(map.entries())
      .map(([key, label]) => ({ key, label }))
      .sort((a, b) => a.label.localeCompare(b.label));
  });

  voicesForProvider: Signal<readonly { key: string; label: string }[]> = computed(() => {
    const providerKey = this.provider();
    if (!providerKey) {
      return [] as const;
    }
    if (providerKey === OPEN_AI_KEY) {
      return OPEN_AI_VOICES;
    }
    const lang = this.language();
    const list = this.narakeetVoices().filter(v => !lang || v.languageCode === lang);
    const capitalized = (s: string) => s ? (s[0].toUpperCase() + s.slice(1)) : s;
    return list.map(v => ({ key: v.name, label: capitalized(v.name) }));
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

  hasResult = signal(false); // Stub: flips after fake submit
  selectedVoiceLabel = computed(() => this.voicesForProvider().find(v => v.key === this.voice())?.label ?? '');
  readonly responseFormats = [
    { key: SpeechResponseFormat.MP3, label: 'MP3' },
    { key: SpeechResponseFormat.Opus, label: 'OPUS' },
    { key: SpeechResponseFormat.AAC, label: 'AAC' },
    { key: SpeechResponseFormat.Flac, label: 'FLAC' },
    { key: SpeechResponseFormat.WAV, label: 'WAV' },
    { key: SpeechResponseFormat.PCM, label: 'PCM' },
  ] as const;

  // 6) Constructor
  constructor() {
    this.translate.onLangChange.subscribe(() => {
      this.langChangeTrigger.update((v) => v + 1);
      this.applyDefaultSampleIfNotEdited();
    });
    this.signalR.startConnection(this.apiBaseUrl);
    this.signalR.addAudioStatusListener((fileId, status, progress, errorMessage) => {
      const id = this.currentFileId();
      if (!id || fileId !== id) return;
      this.status.set(status as AudioStatus);
      this.progress.set(progress ?? 0);
      this.errorMessage.set(errorMessage);
      console.log(`Progress update for ${fileId}: status=${status}, progress=${progress}, error=${errorMessage}`);
      if (status === 'Completed') this.hasResult.set(true);
      if (status === 'Failed') this.openErrorSnackbar('home.progress.failed');
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
  onProviderChange(value: string) {
    const providerKey = (value === OPEN_AI_KEY || value === NARAKEET_KEY) ? (value as ProviderKey) : '';
    this.provider.set(providerKey);
    this.model.set('');
    if (providerKey && Array.isArray(this.providerModels[providerKey]) && this.providerModels[providerKey]!.length > 0) {
      this.model.set(this.providerModels[providerKey]![0]);
    }
    if (providerKey === NARAKEET_KEY) {
      this.language.set('');
      this.http.get<NarakeetVoice[]>('/voices/narakeet').subscribe({
        next: (voices) => this.narakeetVoices.set(voices ?? []),
        error: (error) => {
          console.error('Failed to load Narakeet voices', error);
          this.narakeetVoices.set([]);
        },
      });
    } else {
      // OpenAI does not use language
      this.language.set('');
    }
  }

  onLanguageChange(value: string) {
    this.language.set(value);
    if (this.provider() !== NARAKEET_KEY) {
      return;
    }
    // Refresh the list from backend; voicesForProvider filters by selected language
    this.http.get<NarakeetVoice[]>('/voices/narakeet').subscribe({
      next: (voices) => this.narakeetVoices.set(voices ?? []),
      error: (error) => console.error('Failed to refresh Narakeet voices', error),
    });
  }

  onFileSelected(input: HTMLInputElement) {
    const f = input.files?.[0] ?? null;
    this.file.set(f);
    this.announce(f ? `Selected file ${f.name}` : 'No file selected');
    this.fileTouched.set(true);
  }

  removeFile() {
    this.file.set(null);
    this.announce('File removed');
    this.fileTouched.set(true);
  }

  onDrop(ev: DragEvent) {
    ev.preventDefault();
    const f = ev.dataTransfer?.files?.[0] ?? null;
    if (f) {
      this.file.set(f);
      this.announce(`Selected file ${f.name}`);
    }
    this.fileTouched.set(true);
  }

  onDragOver(ev: DragEvent) { ev.preventDefault(); }

  openFileDialog(input: HTMLInputElement) { input.click(); }

  // Updates the sample text from UI input and marks user-edited state accordingly.
  onSampleTextInput(value: string) {
    this.sampleText.set(value);
    const lastAuto = this.lastAutoSampleText().trim();
    const current = value.trim();
    this.isSampleUserEdited.set(current.length > 0 && current !== lastAuto);
  }

  playOrToggleSample() {
    // Toggle between play/pause/resume based on current state
    const state = this.sampleStatus();
    if (state === 'playing') { this.pauseSample(); return; }
    if (state === 'paused') { this.resumeSample(); return; }
    this.sampleError.set(null);
    // Cancel any inflight request from a previous click
    if (this.sampleRequestSub) {
      try { this.sampleRequestSub.unsubscribe(); } catch { /* noop */ }
      this.sampleRequestSub = undefined;
    }
    if (!this.provider() || (this.provider() === NARAKEET_KEY && !this.language()) || !this.voice()) {
      // Flag sample attempt for field highlighting (but do not mark upload/file errors)
      this.sampleAttempt.set(true);
      // Focus first missing field for better UX
      this.focusSampleMissing();
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
        // Force MP3 for sample per requirements
        responseFormat: SpeechResponseFormat.MP3,
      },
    } as const;
    this.sampleRequestSub = this.tts.getSpeechSample(req).subscribe({
      next: (blob: Blob) => {
        this.sampleAttempt.set(false);
        if (this.sampleUrl) URL.revokeObjectURL(this.sampleUrl);
        this.sampleUrl = URL.createObjectURL(blob);
        this.sampleAudio = new Audio(this.sampleUrl);
        this.sampleAudio.onended = () => this.stopSample();
        this.sampleAudio.onerror = () => { this.sampleError.set('playback'); this.stopSample(); };
        this.sampleAudio.play()
          .then(() => this.sampleStatus.set('playing'))
          .catch((e) => {
            console.error(e);
            this.sampleError.set('permission');
            this.sampleStatus.set('stopped');
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

  pauseSample() {
    try { this.sampleAudio?.pause(); } catch { /* noop */ }
    this.sampleStatus.set('paused');
  }

  resumeSample() {
    try {
      void this.sampleAudio?.play();
      this.sampleStatus.set('playing');
    } catch (e) {
      console.error(e);
      this.sampleStatus.set('stopped');
    }
  }

  stopSample() {
    if (this.sampleRequestSub) {
      try { this.sampleRequestSub.unsubscribe(); } catch { /* noop */ }
      this.sampleRequestSub = undefined;
    }
    this.sampleAudio?.pause();
    if (this.sampleAudio) { this.sampleAudio.src = ''; this.sampleAudio.load(); }
    if (this.sampleUrl) { URL.revokeObjectURL(this.sampleUrl); }
    this.sampleAudio = undefined;
    this.sampleUrl = undefined;
    this.sampleStatus.set('stopped');
  }

  submit() {
    this.submitAttempt.set(true);
    if (!this.formValid()) { this.focusFirstInvalid(); return; }
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
    this.status.set('Created');
    this.progress.set(0);
    this.hasResult.set(false);
    this.tts.createSpeech(req).subscribe({
      next: (id: string) => {
        this.currentFileId.set(id);
      },
      error: () => {
        this.status.set('Failed');
        this.openErrorSnackbar('home.errors.failed');
      },
    });
  }

  clear(formEl?: HTMLFormElement) {
    // Stop any ongoing sample playback and clear errors
    this.stopSample();
    this.sampleError.set(null);

    this.provider.set('');
    this.model.set('');
    this.language.set('');
    this.voice.set('');
    this.speed.set(1.0);
    this.responseFormat.set(SpeechResponseFormat.MP3);
    this.file.set(null);
    this.hasResult.set(false);
    // Reset sample text back to its default and mark as not user-edited
    const initialSample = this.resolveDefaultSampleText();
    this.sampleText.set(initialSample);
    this.lastAutoSampleText.set(initialSample);
    this.isSampleUserEdited.set(false);
    formEl?.reset();
    this.announce('Form reset');
    this.submitAttempt.set(false);
    this.fileTouched.set(false);
    this.status.set('Idle');
    this.progress.set(0);
    this.currentFileId.set(null);
  }

  download() {
    const id = this.currentFileId();
    if (!id) return;
    this.tts.downloadById(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; 
        const name = this.file()!.name;
        const dotIndex = name.lastIndexOf('.');
        const baseName = name.substring(0, dotIndex);
        a.download = `${baseName}.${this.responseFormat()}`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 0);
      },
      error: () => this.openErrorSnackbar('home.progress.failed'),
    });
  }

  cancel() {
    const id = this.currentFileId();
    if (id) this.signalR.cancelProcessing(id);
  }

  formatSpeed = (v: number | null) => (v == null ? '' : v.toFixed(1));

  // 9) Private helpers
  private announce(msg: string) {
    this.statusMessage.set(msg);
    // Clear after a while so subsequent messages are re-announced
    queueMicrotask(() => this.statusMessage.set(''));
  }

  private focusFirstInvalid() {
    queueMicrotask(() => {
      if (!this.provider()) { this.providerEl?.focus(); return; }
      if (this.requiresModel() && !this.model()) { this.modelEl?.focus(); return; }
      if (this.provider() === NARAKEET_KEY && !this.language()) { this.languageEl?.focus(); return; }
      if (!this.voice()) { this.voiceEl?.focus(); return; }
      if (!this.file()) { this.fileInput?.nativeElement.focus(); return; }
    });
  }

  private openErrorSnackbar(messageKey: string) {
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

  // Icon helper for progress header to keep template tidy
  progressIcon(): string {
    const s = this.status();
    if (s === 'Created') {
      return 'schedule';
    }
    if (s === 'Processing') {
      return 'autorenew';
    }
    if (s === 'Completed') {
      return 'check_circle';
    }
    if (s === 'Failed') {
      return 'error';
    }
    if (s === 'Canceled') {
      return 'cancel';
    }
    return 'info';
  }

  private focusSampleMissing() {
    queueMicrotask(() => {
      if (!this.provider()) { this.providerEl?.focus(); return; }
      if (this.provider() === NARAKEET_KEY && !this.language()) { this.languageEl?.focus(); return; }
      if (!this.voice()) { this.voiceEl?.focus(); return; }
    });
  }
}

type AudioStatus = 'Idle' | 'Created' | 'Processing' | 'Completed' | 'Failed' | 'Canceled';
