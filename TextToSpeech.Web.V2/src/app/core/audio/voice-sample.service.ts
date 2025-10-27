import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class VoiceSampleService {
  readonly current = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly status = signal<'stopped' | 'playing' | 'paused'>('stopped');

  private audio?: HTMLAudioElement;
  private objectUrl?: string;
  private inflight?: AbortController;

  // If you need mapping, keep it; otherwise remove.
  private readonly voiceMap: Record<string, string> = {
    'female-a': 'female-a',
    'female-b': 'female-b',
    'male-a': 'male-a',
    'male-b': 'male-b',
  };

  isPlaying(key: string) { return this.current() === key && this.status() === 'playing'; }
  isPaused(key: string) { return this.current() === key && this.status() === 'paused'; }

  playToggle(key: string) {
    if (this.current() === key) {
      if (this.status() === 'playing') { this.pause(); return; }
      if (this.status() === 'paused') { this.resume(); return; }
    }
    this.play(key);
  }

  async play(key: string) {
    this.stop(); // stop any previous playback/request
    this.error.set(null);
    this.current.set(key);

    // Build request
    const voiceId = this.voiceMap[key] ?? key;
    this.inflight = new AbortController();

    try {
      const res = await fetch('/speech/sample', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ voice: voiceId }), // adjust payload shape if API needs more
        signal: this.inflight.signal,
      });

      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const blob = await res.blob(); // expect audio/* (mp3/wav/ogg)
      // Create object URL and play
      this.objectUrl = URL.createObjectURL(blob);
      this.audio = new Audio(this.objectUrl);
      this.audio.preload = 'auto';
      this.audio.onended = () => this.stop();
      this.audio.onerror = () => { this.error.set('playback'); this.stop(); };

      await this.audio.play();
      this.status.set('playing');
    } catch (e) {
      console.error(e);
      if (e instanceof DOMException && e.name === 'AbortError') {
        // ignore aborts (e.g., user switched voices quickly)
        return;
      }
      this.error.set('request'); // or set to e.message for debugging
      this.current.set(null);
      this.status.set('stopped');
    } finally {
      this.inflight = undefined;
    }
  }

  stop() {
    // cancel pending request
    try { this.inflight?.abort(); } catch { void 0; }
    this.inflight = undefined;

    // stop audio
    try {
      if (this.audio) {
        this.audio.pause();
        this.audio.src = '';
        this.audio.load();
      }
    } catch (err) { console.error(err); }
    if (this.objectUrl) { URL.revokeObjectURL(this.objectUrl); }
    this.objectUrl = undefined;
    this.audio = undefined;

    this.current.set(null);
    this.status.set('stopped');
  }

  pause() {
    try { this.audio?.pause(); } catch (err) { console.error(err); }
    this.status.set('paused');
  }

  resume() {
    try { void this.audio?.play(); } catch (err) { console.error(err); }
    this.status.set('playing');
  }
}
