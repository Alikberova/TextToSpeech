import { Component } from '@angular/core';
import { AppMaterialModule } from '../app.material/app.material.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-tts-form',
  standalone: true,
  imports: [ FormsModule, AppMaterialModule, CommonModule, RouterOutlet ],
  templateUrl: './tts-form.component.html',
  styleUrl: './tts-form.component.scss'
})
export class TtsFormComponent {
  voices = ['Voice 1', 'Voice 2', 'Voice 3', 'Voice 4', 'Voice 5', 'Voice 6'];
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
}
