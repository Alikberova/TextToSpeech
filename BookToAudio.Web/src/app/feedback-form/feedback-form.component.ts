import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FeedbackService } from '../services/feedback.service';

@Component({
  selector: 'app-feedback-form',
  standalone: true,
  imports: [MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, FormsModule],
  templateUrl: './feedback-form.component.html',
  styleUrl: './feedback-form.component.scss'
})
export class FeedbackFormComponent {
feedback = { name: "lal", userEmail: "qwerty@q.com", message: "Cool service!" };

  constructor(private feedbackService: FeedbackService) { }

  submitFeedback() {
    this.feedbackService.feedbackMessageSend(this.feedback).subscribe({
      next: res => console.log(`status OK 200 \n Ressponce: \p ${res}`),
      error: err=> console.log(`Error: \p ${err}`)
    })
  }
}
