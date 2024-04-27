import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToggleButtonComponent } from '../toggle-button/toggle-button.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ValidationService } from '../../services/validation.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth-form',
  standalone: true,
  imports: [MatIconModule, MatInputModule, FormsModule, ToggleButtonComponent, MatFormFieldModule, ReactiveFormsModule, CommonModule],
  templateUrl: './auth-form.component.html',
  styleUrl: './auth-form.component.scss'
})
export class AuthFormComponent {
  authForm: FormGroup;

  constructor() {
    this.authForm = new FormGroup({
      userName: new FormControl("", Validators.email),
      password: new FormControl("", ValidationService.matchRequiredPassword)
    })
  }

  @Input() title: string = '';
  @Input() buttonLabel: string = '';
  @Output() formSubmitted = new EventEmitter<{ userName: string; password: string }>();
  @Output() toggleChanged = new EventEmitter<boolean>();
  @Input() isRegister: boolean = false;

  userName: string = '';
  password: string = '';
  hidePassword = true;

  onSubmit(): void {
    this.formSubmitted.emit({ userName: this.userName, password: this.password });
  }

  onToggleChange(newState: boolean): void {
    this.isRegister = newState;
    this.toggleChanged.emit(newState);
  }
}
