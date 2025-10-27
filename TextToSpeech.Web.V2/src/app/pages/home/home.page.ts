import { Component, Signal, ViewChild, ElementRef, OnDestroy, computed, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelect, MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NARAKEET_KEY, OPEN_AI_KEY, OPEN_AI_VOICES, PROVIDER_MODELS, ProviderKey, PROVIDERS } from '../../constants/tts-constants';
import { NarakeetVoice } from '../../dto/narakeet-voice';
import { LANGUAGES } from '../../constants/language';
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
    TranslateModule,
  ],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePage implements OnDestroy {
  private readonly translate = inject(TranslateService);
  private readonly http = inject(HttpClient);
  private readonly tts = inject(TtsService);
  private readonly snack = inject(MatSnackBar);
  private readonly signalR = inject(SignalRService);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly narakeetVoices = signal<NarakeetVoice[]>([]);
  private readonly langChangeTrigger = signal(0);

  readonly providers = PROVIDERS;
  readonly providerModels = PROVIDER_MODELS;
  // Sorted by current locale (e.g., 'uk' vs 'en') and reactive to language changes
  readonly languages = computed(() => {
    this.langChangeTrigger();
    const current = this.translate.currentLang || this.translate.getDefaultLang() || 'en';
    return [...LANGUAGES].sort((a, b) => {
      const aLabel = this.translate.instant(a.label);
      const bLabel = this.translate.instant(b.label);
      return aLabel.localeCompare(bLabel, current);
    });
  });

  constructor() {
    this.translate.onLangChange.subscribe(() => {
      this.langChangeTrigger.update((v) => v + 1);
    });
    // Start SignalR connection once for status updates
    this.signalR.startConnection(this.apiBaseUrl);
    this.signalR.addAudioStatusListener((fileId, status, progress, errorMessage) => {
      if (!this.currentFileId() || fileId !== this.currentFileId()) return;
      this.status.set(status as AudioStatus);
      this.progress.set(progress ?? 0);
      this.errorMessage.set(errorMessage);
      if (status === 'Completed') {
        this.hasResult.set(true);
      }
      if (status === 'Failed') {
        this.openErrorSnackbar('home.progress.failed');
      }
    });
  }
  
  // Form signals
  provider = signal<ProviderKey | ''>('');
  model = signal<string | ''>('');
  language = signal<string | ''>('');
  voice = signal<string | ''>('');
  speed = signal<number>(1.0);
  file = signal<File | null>(null);
  statusMessage = signal<string>(''); // for ARIA live announcements
  submitAttempt = signal(false); // after first submit click
  fileTouched = signal(false); // for per-field error behavior on file

  voicesForProvider: Signal<readonly { key: string; label: string }[]> = computed(() => {
    const providerKey = this.provider();
    if (!providerKey) return [] as const;
    if (providerKey === OPEN_AI_KEY) return OPEN_AI_VOICES;
    // narakeet mapping
    return this.narakeetVoices().map(v => ({ key: v.name, label: v.name }));
  });

  // Derived signals
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
    const languageOk = !!this.language();
    const voiceOk = !!this.voice();
    const fileOk = !!this.file();
    return providerOk && modelOk && languageOk && voiceOk && fileOk;
  });

  hasResult = signal(false); // Stub: flips after fake submit
  selectedVoiceLabel = computed(() => this.voicesForProvider().find(v => v.key === this.voice())?.label ?? '');
  readonly fileInputId = 'fileInputEl';
  readonly speedInputId = 'speedInputEl';

  // Sample playback state
  sampleText = signal<string>('Lorem ipsum');
  samplePlaying = signal<boolean>(false);
  sampleError = signal<string | null>(null);
  private sampleAudio?: HTMLAudioElement;
  private sampleUrl?: string;

  // Generation state
  private currentFileId = signal<string | null>(null);
  status = signal<AudioStatus>('Idle');
  progress = signal<number>(0);
  errorMessage = signal<string | undefined>(undefined);

  // Keep aria-live messages terse and contextual
  private announce(msg: string) {
    this.statusMessage.set(msg);
    // Clear after a while so subsequent messages are re-announced
    queueMicrotask(() => this.statusMessage.set(''));
  }

  private focusFirstInvalid() {
    queueMicrotask(() => {
      if (!this.provider()) { this.providerEl?.focus(); return; }
      if (this.requiresModel() && !this.model()) { this.modelEl?.focus(); return; }
      if (!this.language()) { this.languageEl?.focus(); return; }
      if (!this.voice()) { this.voiceEl?.focus(); return; }
      if (!this.file()) { this.fileInput?.nativeElement.focus(); return; }
    });
  }

  // File input interaction
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

  // Drag / drop with keyboard support
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

  // Actions
  submit() {
    this.submitAttempt.set(true);
    if (!this.formValid()) { this.focusFirstInvalid(); return; }
    // Build request and start server-side generation
    const req = {
      ttsApi: this.provider(),
      languageCode: this.language(),
      model: this.requiresModel() ? this.model() : undefined,
      speed: this.speed(),
      voice: this.voice(),
      file: this.file()!,
      input: undefined,
    } as const;

    this.status.set('Created');
    this.progress.set(0);
    this.hasResult.set(false);
    this.tts.createSpeech(req).subscribe({
      next: (id: string) => {
        this.currentFileId.set(id);
        // Progress will be updated by SignalR listener
      },
      error: () => {
        this.status.set('Failed');
        this.openErrorSnackbar('home.errors.failed');
      },
    });
  }

  clear(formEl?: HTMLFormElement) {
    this.provider.set('');
    this.model.set('');
    this.language.set('');
    this.voice.set('');
    this.speed.set(1.0);
    this.file.set(null);
    this.hasResult.set(false);
    formEl?.reset();
    this.announce('Form reset');
    this.submitAttempt.set(false);
    this.fileTouched.set(false);
    this.status.set('Idle');
    this.progress.set(0);
    this.currentFileId.set(null);
  }

  onProviderChange(value: string) {
    const providerKey = (value === OPEN_AI_KEY || value === NARAKEET_KEY) ? (value as ProviderKey) : '';
    this.provider.set(providerKey);
    this.model.set('');
    // Auto-select first available model when provider requires it
    if (providerKey && Array.isArray(this.providerModels[providerKey]) && this.providerModels[providerKey]!.length > 0) {
      this.model.set(this.providerModels[providerKey]![0]);
    }
    // Load Narakeet voices once when selected
    if (providerKey === NARAKEET_KEY && this.narakeetVoices().length === 0) {
      this.http.get<NarakeetVoice[]>('/voices/narakeet').subscribe({
        next: (voices) => this.narakeetVoices.set(voices ?? []),
        error: (error) => {
          console.error('Failed to load Narakeet voices', error);
          this.narakeetVoices.set([]);
        },
      });
    }
  }

  formatSpeed = (v: number | null) => (v == null ? '' : v.toFixed(1));

  // References for focusing the first invalid control
  @ViewChild('providerEl') private providerEl?: MatSelect;
  @ViewChild('modelEl') private modelEl?: MatSelect;
  @ViewChild('languageEl') private languageEl?: MatSelect;
  @ViewChild('voiceEl') private voiceEl?: MatSelect;
  @ViewChild('fileInput') private fileInput?: ElementRef<HTMLInputElement>;

  // Text sample playback
  playOrToggleSample() {
    if (this.samplePlaying()) { this.stopSample(); return; }
    this.sampleError.set(null);
    // Require voice, language, provider context to preview accurately
    if (!this.provider() || !this.language() || !this.voice()) {
      this.openErrorSnackbar('home.errors.required');
      return;
    }
    const req = {
      ttsApi: this.provider()!,
      languageCode: this.language()!,
      model: this.requiresModel() ? this.model() || undefined : undefined,
      speed: this.speed(),
      voice: this.voice()!,
      input: this.sampleText(),
    };
    this.tts.getSpeechSample(req).subscribe({
      next: (blob: Blob) => {
        // Revoke previous object URL to avoid leaks
        if (this.sampleUrl) {
          URL.revokeObjectURL(this.sampleUrl);
        }
        this.sampleUrl = URL.createObjectURL(blob);
        this.sampleAudio = new Audio(this.sampleUrl);
        this.sampleAudio.onended = () => this.stopSample();
        this.sampleAudio.onerror = () => { this.sampleError.set('playback'); this.stopSample(); };
        // In headless browsers, play() may reject without a user gesture.
        // Handle gracefully to avoid unhandled promise rejections in tests.
        this.sampleAudio.play()
          .then(() => this.samplePlaying.set(true))
          .catch((e) => {
            console.error(e);
            this.sampleError.set('permission');
            this.samplePlaying.set(false);
          });
      },
      error: () => {
        this.sampleError.set('request');
        this.openErrorSnackbar('home.voice.sampleError');
      },
    });
  }

  stopSample() {
    // Stop and reset the audio element if present
    this.sampleAudio?.pause();
    if (this.sampleAudio) { this.sampleAudio.src = ''; this.sampleAudio.load(); }
    if (this.sampleUrl) { URL.revokeObjectURL(this.sampleUrl); }
    this.sampleAudio = undefined;
    this.sampleUrl = undefined;
    this.samplePlaying.set(false);
  }

  download() {
    const id = this.currentFileId();
    if (!id) return;
    this.tts.downloadById(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = `speech-${id}.mp3`;
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

  private openErrorSnackbar(messageKey: string) {
    const message = this.translate.instant(messageKey);
    this.snack.open(message, undefined, { duration: 4000 });
  }

  ngOnDestroy(): void {
    this.stopSample();
  }
}

type AudioStatus = 'Idle' | 'Created' | 'Processing' | 'Completed' | 'Failed' | 'Canceled';
