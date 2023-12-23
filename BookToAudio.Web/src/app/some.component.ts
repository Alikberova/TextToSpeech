// some.component.ts
import { Component } from '@angular/core';
import { OpenaiService } from './services/openai.service'

@Component({
  selector: 'app-some',
  template: '<button (click)="generateSpeech()">Generate Speech</button>',
})
export class SomeComponent {
  constructor(private openaiService: OpenaiService) {}

  generateSpeech() {
    const modelConfig = {
      model: 'custom-tts-model',
      voice: 'custom-voice',
      format: 'wav',
      speed: 'fast',
    };

    this.openaiService.generateSpeech(modelConfig).subscribe(
      (response) => {
        console.log('Speech generation successful', response);
        // Handle success
      },
      (error) => {
        console.error('Speech generation failed', error);
        // Handle error
      }
    );
  }
}
