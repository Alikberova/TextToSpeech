// A lightweight audio player wrapper for sample playback logic.
// Manages object URL lifetime and provides simple controls.

export class SampleAudioPlayer {
  private audio?: HTMLAudioElement;
  private url?: string;

  constructor(private readonly onEnded: () => void, private readonly onError: () => void) {}

  setBlob(blob: Blob): void {
    this.disposeUrl();
    this.url = URL.createObjectURL(blob);
    this.audio = new Audio(this.url);
    this.audio.onended = () => this.onEnded();
    this.audio.onerror = () => this.onError();
  }

  async play(): Promise<void> {
    if (!this.audio) {
      return;
    }
    await this.audio.play();
  }

  pause(): void {
    try {
      this.audio?.pause();
    } catch {
      // no-op
    }
  }

  async resume(): Promise<void> {
    await this.play();
  }

  stop(): void {
    try {
      this.audio?.pause();
    } catch {
      // no-op
    }
    if (this.audio) {
      this.audio.src = '';
      this.audio.load();
    }
    this.disposeUrl();
    this.audio = undefined;
  }

  private disposeUrl(): void {
    if (this.url) {
      URL.revokeObjectURL(this.url);
      this.url = undefined;
    }
  }
}
