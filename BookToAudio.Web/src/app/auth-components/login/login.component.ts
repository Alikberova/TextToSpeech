import { Component } from '@angular/core';
import { AuthFormComponent } from "../auth-form/auth-form.component";
import { UserClient } from '../../http-clients/user-client';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { SnackbarService } from '../../shared-ui/snackbar-service';

@Component({
    selector: 'app-login',
    standalone: true,
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
    imports: [AuthFormComponent]
})
export class LoginComponent {
  constructor(private userClient: UserClient, private authService: AuthService,
    private router: Router, private snackbarService: SnackbarService) {}

  title = 'Login';
  buttonLabel = 'Sign In';

  onFormSubmitted(authData: { userName: string; password: string }): void {
    this.userClient.checkIfUserExists(authData.userName)
    .subscribe({
      next: (exists) => exists ?
        this.authenticateUser(authData.userName, authData.password) :
        this.snackbarService.showError("User does not exist")
        ,
      error: (e) => console.error(e)
    })
  }

  authenticateUser(userName: string, password: string) {
    const credentials = { username: userName, password: password };
    this.userClient.loginUser(credentials).subscribe({
      next: (response) => this.handleAuthenticationSuccess(response.token),
      error: (error) => this.snackbarService.showError(error)
    })
  }

  handleAuthenticationSuccess(token: string) {
    this.authService.setToken(token);
    this.router.navigate(['home'])
  }

  handleToggle(isRegister: boolean): void {
    if (isRegister) {
      this.router.navigate(['register']);
    }
  }
}
