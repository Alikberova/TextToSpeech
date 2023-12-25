import { Component } from '@angular/core';
import { AuthFormComponent } from "../auth-form/auth-form.component";
import { UserClient } from '../http-clients/user-client';
import { AuthService } from '../services/auth/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
    imports: [AuthFormComponent]
})
export class LoginComponent {
  constructor(private userClient: UserClient, private authService: AuthService) {}

  title = 'Login';
  buttonLabel = 'Sign In';

  onFormSubmitted(authData: { userName: string; password: string }): void {
    this.userClient.checkIfUserExists(authData.userName)
    .subscribe({
      next: (exists) => exists ?
        this.authenticateUser(authData.userName, authData.password) :
        console.error("User does not exist. Show an error message. todo"),
      error: (e) => console.error(e)
    })
  }

  authenticateUser(userName: string, password: string) {
    const credentials = { username: userName, password: password };
    this.userClient.loginUser(credentials).subscribe({
      next: (response) => this.handleAuthenticationSuccess(response.token),
      error: (error) => this.handleAuthenticationFailure(error)
    })
  }

  handleAuthenticationSuccess(token: string) {
    this.authService.setToken(token);
    //todo redirect to home page
  }

  handleAuthenticationFailure(error: any) {
    console.error('Login failed. Show an error message.', error)
    //todo show error message
  }
}
