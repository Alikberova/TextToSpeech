import { Component } from '@angular/core';
import { AuthFormComponent } from "../auth-form/auth-form.component";
import { AuthService } from '../services/auth/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
    imports: [AuthFormComponent]
})
export class LoginComponent {
  title = 'Login';
  buttonLabel = 'Sign In';

  onFormSubmitted(authData: { userName: string; password: string }): void {
    this.authService.checkIfUserExists(authData.userName)
    .subscribe({
      next: (exists) => exists ?
        this.authenticateUser(authData.userName, authData.password) :
        console.error("User does not exist. Show an error message. todo"),
      error: (e) => console.error(e)
    })
  }

  constructor(private authService: AuthService) {}

  authenticateUser(userName: string, password: string) {
    const credentials = { username: userName, password: password };
    this.authService.loginUser(credentials).subscribe({
      next: () => console.log('Login successful. Redirect to home'),
      error: (e) => console.error('Login failed. Show an error message. todo', e)
    })
  }
}
