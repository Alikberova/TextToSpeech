import { Component, ViewChild } from '@angular/core';
import { FormControl, FormGroup, NgForm, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FeedbackService } from '../services/feedback.service';
import { ValidationService } from '../services/validation.service';
import { CommonModule } from '@angular/common';
import { SnackbarService } from '../shared-ui/snackbar-service';

@Component({
  selector: 'app-feedback-form',
  standalone: true,
  imports: [MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-form.component.html',
  styleUrl: './feedback-form.component.scss'
})
export class FeedbackFormComponent {
  feedbackForm: FormGroup;
  @ViewChild('formDirective') private formDirective!: NgForm;

  constructor(private feedbackService: FeedbackService, private snackbarService: SnackbarService) {
    this.feedbackForm = new FormGroup({
      userEmail: new FormControl("", [ValidationService.validationEmail, Validators.required]),
      name: new FormControl("", Validators.required),
      message: new FormControl("", Validators.required),
    });
  }

  submitFeedback() {
    if (this.feedbackForm.valid) {
     const feedback = this.feedbackForm.value;
      this.feedbackService.feedbackMessageSend(feedback).subscribe({
        next: _ => {
          this.snackbarService.showSuccess("Your feedback was sent. Thanks!")
          this.feedbackForm.reset();
          this.formDirective.resetForm();
        },
        error: err => {
          console.log(err)
          this.snackbarService.showError("Oops. Some error occurred, couldn't process your request. Try to reload the page", undefined, 10000)
        }
      });
    }
  }
}
