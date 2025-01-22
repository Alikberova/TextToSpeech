import { Component, ViewChild } from '@angular/core';
import { FormControl, FormGroup, NgForm, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { EmailClient } from '../../http-clients/email-client';
import { SnackbarService } from '../../ui-services/snackbar-service';

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
  isSubmitted = false;

  constructor(private emailClient: EmailClient, private snackbarService: SnackbarService) {
    this.feedbackForm = new FormGroup({
      userEmail: new FormControl("", [Validators.email]),
      name: new FormControl(""),
      message: new FormControl(""),
    });
  }

  submitFeedback() {
    if (this.feedbackForm.valid) {
      const feedback = this.feedbackForm.value;
      this.emailClient.sendEmail(feedback).subscribe({
        next: _ => {
          this.snackbarService.showSuccess("Your feedback was sent. Thanks!")
          this.feedbackForm.reset();
          this.formDirective.resetForm();
        }
      });
    }
  }
}
