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
  //todo delete id 
  voices = [
    { id: 'alloy', name: 'Alloy' },
    { id: 'echo', name: 'Echo' },
    { id: 'fable', name: 'Fable' },
    { id: 'onyx', name: 'Onyx' },
    { id: 'nova', name: 'Nova' },
    { id: 'shimmer', name: 'Shimmer' },
  ];

  selectedVoice = 'Alloy';

  speed = 1;
  chosenFile: File = null!;

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.chosenFile = target.files[0];
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
    this.playSemple(testParams);
  }

  playSemple(param: TextToSpeech) {
    this.openAiClient.createSpeech(param).subscribe((blob) => {
      const audio = new Audio();
      const url = URL.createObjectURL(blob);
      audio.src = url;
      audio.load();
      audio
        .play()
        .catch((error) => console.log('error to in textToSpeech().', error));
    });
  }
}

//==============================================================

// import { Component } from '@angular/core';
// import { AppMaterialModule } from '../app.material/app.material.module';
// import { FormsModule } from '@angular/forms';
// import { CommonModule } from '@angular/common';
// import { RouterOutlet } from '@angular/router';
// import { AudioClient } from '../http-clients/audio-client';
// import { Openai } from '../http-clients/openai-client';
// import { TextToSpeech } from '../models/text-to-speech';

// @Component({
//   selector: 'app-tts-form',
//   standalone: true,
//   imports: [FormsModule, AppMaterialModule, CommonModule, RouterOutlet],
//   templateUrl: './tts-form.component.html',
//   styleUrl: './tts-form.component.scss',
// })
// export class TtsFormComponent {
//   constructor(
//     private audioService: AudioClient,
//     private openAiClient: Openai
//   ) {}

//   voices = [
//     { id: '9', name: 'Voice 1' },
//     { id: '40', name: 'Voice 2' } /* ... other voices ... */,
//   ];
//   private audio = new Audio();
//   selectedVoice = 'Voice 1';

//   speed = 1;
//   chosenFile: File = null!;

//   onFileSelected(event: Event) {
//     debugger;
//     const target = event.target as HTMLInputElement;
//     if (target.files && target.files.length > 0) {
//       this.chosenFile = target.files[0];
//       console.log(this.chosenFile);
//     }
//   }

//   textToSpeech(param: TextToSpeech) {
//     this.openAiClient.createSpeech(param).subscribe((blob) => {
//       const audio = new Audio();
//       const url = URL.createObjectURL(blob);
//       audio.load();
//       audio.src = url;
//       audio
//         .play()
//         .catch((error) => console.log('error to in textToSpeech().', error));
//     });
//   }
//   playVoiceSample(event: MouseEvent, voice: any): void {
//     event.stopPropagation(); // Prevent the mat-select from changing its value
//     // todo replace with openai api
//     const audioUrl = this.audioService.getAudioStreamUrl(voice.id);
//     this.audio.src = audioUrl;
//     this.audio.load();
//     this.audio.play();
//   }
// }
