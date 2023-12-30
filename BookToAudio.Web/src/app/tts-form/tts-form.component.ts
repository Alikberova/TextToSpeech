import { Component } from '@angular/core';
import { AppMaterialModule } from '../app.material/app.material.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AudioClient } from '../http-clients/audio-client';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [ FormsModule, AppMaterialModule, CommonModule, RouterOutlet ],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss'
})
export class TtsFormComponent {
  constructor(private audioService: AudioClient) {}

  voices = [{ id: '9', name: 'Voice 1' }, { id: '40', name: 'Voice 2' }/* ... other voices ... */];
  private audio = new Audio();
  selectedVoice = 'Voice 1';
  
  speed = 1;
  chosenFile: File = null!;

  onFileSelected(event: Event) {
    debugger;
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.chosenFile = target.files[0];
      console.log(this.chosenFile)
    }
  }

  playVoiceSample(event: MouseEvent, voice: any): void {
    event.stopPropagation(); // Prevent the mat-select from changing its value
//todo replace with openai api
    const audioUrl = this.audioService.getAudioStreamUrl(voice.id);
    this.audio.src = audioUrl;
    this.audio.load();
    this.audio.play();
  }
}
