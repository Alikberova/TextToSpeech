import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppMaterialModule } from '../app.material/app.material.module';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-auth-form',
  standalone: true,
  imports: [ AppMaterialModule, FormsModule ],
  templateUrl: './auth-form.component.html',
  styleUrl: './auth-form.component.scss'
})
export class AuthFormComponent {
  @Input() title: string = '';
  @Input() buttonLabel: string = '';
  @Output() formSubmitted = new EventEmitter<{ userName: string; password: string }>();

  userName: string = '';
  password: string = '';

  onSubmit(): void {
    this.formSubmitted.emit({ userName: this.userName, password: this.password });
  }
}
