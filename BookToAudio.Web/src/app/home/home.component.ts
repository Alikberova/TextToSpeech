import { Component } from '@angular/core';
import { TtsFormComponent } from '../tts-form/tts-form.component';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [ TtsFormComponent ]
})
export class HomeComponent {

}
