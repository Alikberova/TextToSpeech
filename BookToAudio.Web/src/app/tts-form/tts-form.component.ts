import { Component } from '@angular/core';
import { AppMaterialModule } from '../app.material/app.material.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { OpenaiClient } from '../http-clients/openai-client';
import { TextToSpeech } from '../models/text-to-speech';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [FormsModule, AppMaterialModule, CommonModule, RouterOutlet],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss',
})
export class TtsFormComponent {
  constructor(private openAiClient: OpenaiClient) {}

  voices = [
    { name: 'alloy' },
    { name: 'echo' },
    { name: 'fable' },
    { name: 'onyx' },
    { name: 'nova' },
    { name: 'shimmer' },
  ];

  selectedVoice = 'Alloy';
  speed = 1;
  chosenFile: File = null!;
  chosenFileName = '';

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.chosenFile = target.files[0];
      this.chosenFileName = this.chosenFile.name;
      console.log(this.chosenFile);
    }
  }
  
  //todo change input "now time"
  playVoiceSample(event: MouseEvent, voiceId: string, speed: number): void {
    event.stopPropagation();
    
    const testParams: TextToSpeech = {
      model: 'tts-1',
      voice: voiceId,
      speed: speed,
      input: 'это мой голос',
    };
    this.playSample(testParams);
  }

  playSample(param: TextToSpeech) {
    this.openAiClient.createSpeech(param).subscribe((blob: Blob) => {
      const audio = new Audio();
      const url = URL.createObjectURL(blob);
      audio.src = url;
      audio.load();
      audio
        .play()
        .catch((error) => console.log('error to in textToSpeech().', error));
    });
  }

  clearFileSelection() {
    this.chosenFile = null!;
    this.chosenFileName = '';
  }
}
