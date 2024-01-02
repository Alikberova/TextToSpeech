import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppMaterialModule } from '../../app.material/app.material.module';
import { FormsModule } from '@angular/forms';
import { ToggleButtonComponent } from '../toggle-button/toggle-button.component';

@Component({
  selector: 'app-auth-form',
  standalone: true,
  imports: [ AppMaterialModule, FormsModule, ToggleButtonComponent ],
  templateUrl: './auth-form.component.html',
  styleUrl: './auth-form.component.scss'
})
export class AuthFormComponent {
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
