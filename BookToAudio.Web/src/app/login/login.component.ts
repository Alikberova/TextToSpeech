import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppMaterialModule } from '../app.material/app.material.module';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ FormsModule, AppMaterialModule ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username: string = '';
  password: string = '';

  onLogin() {
  }
}
