import { Component } from '@angular/core';
import { AuthFormComponent } from '../auth-form/auth-form.component';
import { UserClient } from '../../http-clients/user-client';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { ErrorHandlerService } from '../../services/error-handler-service';
import { RoutesConstants } from '../../constants/route-constants';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [AuthFormComponent],
})
export class LoginComponent {
  constructor(private userClient: UserClient, private authService: AuthService,
    private router: Router, private errorHandler: ErrorHandlerService) {}

  title = 'Login';
  buttonLabel = 'Sign In';

  onFormSubmitted(credentials: { userName: string; password: string }): void {
    this.userClient.loginUser(credentials)
    .subscribe({
      next: (response) => this.handleAuthenticationSuccess(response.token),
      error: (e) => this.errorHandler.handleHttpError(e)
    })
  }

  handleAuthenticationSuccess(token: string) {
    this.authService.setToken(token);
    this.router.navigate(['home']);
  }

  handleToggle(isRegister: boolean): void {
    if (isRegister) {
      this.router.navigate([RoutesConstants.register]);
    }
  }
}
