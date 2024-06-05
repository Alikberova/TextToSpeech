import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
  })
  export class AudioService {
    private audio: HTMLAudioElement | null = null;
  
    playAudio(blob: Blob, onPlay: () => void, onEnd: () => void): void {
      this.audio = new Audio();
      this.audio.src = URL.createObjectURL(blob);
      this.audio.load();
      this.audio.oncanplay = () => {
        this.audio!.play().then(onPlay);
      }
      this.audio.onended = onEnd;
    }

    pauseAudio(): void {
        if (this.audio && !this.audio.paused) {
            this.audio.pause();
        }
    }

    play(): void {
        if (this.audio) {
            this.audio.play();
        }
    }
  
    isPlaying(): boolean {
      return this.audio !== null && !this.audio.paused;
    }

    revokeAudioSample() { //todo rename to stopAudio
        if (!this.audio) {
          return;
        }
        this.audio.pause();
        // this.setCurrentlyPlayingData(null, null, null);
        URL.revokeObjectURL(this.audio.currentSrc);
        this.audio = null;
    }
}
  