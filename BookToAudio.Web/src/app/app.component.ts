import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

import { AppNavComponent } from "./app-nav/app-nav.component";
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "./footer/footer.component";
import { HomeComponent } from './home/home.component';
import { FeedbackFormComponent } from './feedback-form/feedback-form.component';

@Component({
    selector: 'app-root',
    standalone: true,
    templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [CommonModule, FormsModule, RouterOutlet, AppNavComponent, HomeComponent, FooterComponent]
})
export class AppComponent {
}
