import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FeedbackService } from '../services/feedback.service';
import { ValidationService } from '../services/validation.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-feedback-form',
  standalone: true,
  imports: [MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-form.component.html',
  styleUrl: './feedback-form.component.scss'
})
export class FeedbackFormComponent {
  feedbackForm: FormGroup;

  constructor(private feedbackService: FeedbackService) {
    this.feedbackForm = new FormGroup({
      userEmail: new FormControl("",   ValidationService.validationEmail),
      name: new FormControl("", Validators.required),
      message: new FormControl("", Validators.required),
    });
    this.feedbackForm.setValue({
      name: "lal", userEmail: "werty@q.com", message: "Cool service!"
    });
  }

  submitFeedback() {
    if (this.feedbackForm.valid) {
     const feedback = this.feedbackForm.value;
      this.feedbackService.feedbackMessageSend(feedback).subscribe({
        next: res => console.log(`status OK 200 \n Ressponce: \p ${res}`),
        error: err => console.log(`Error: \p ${err}`)
      });
    }
  }
}
