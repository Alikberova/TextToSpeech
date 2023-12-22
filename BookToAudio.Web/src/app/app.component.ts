import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

import { AppNavComponent } from "./app-nav/app-nav.component";
import { TtsFormComponent } from "./tts-form/tts-form.component";
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "./footer/footer.component";

@Component({
    selector: 'app-root',
    standalone: true,
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [CommonModule, FormsModule, RouterOutlet, AppNavComponent, TtsFormComponent, FooterComponent]
})
export class AppComponent {

}
