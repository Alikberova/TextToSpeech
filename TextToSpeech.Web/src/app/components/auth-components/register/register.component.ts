import { Component } from '@angular/core';
import { AuthFormComponent } from "../auth-form/auth-form.component";
import { Router } from '@angular/router';
import { UserClient } from '../../../http-clients/user-client';
import {User} from '../../../models/dto/user'
import { SnackbarService } from '../../../shared-ui/snackbar-service';
import { RoutesConstants } from '../../../constants/route-constants';

@Component({
    selector: 'app-register',
    standalone: true,
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
    imports: [AuthFormComponent]
})

export class RegisterComponent {
  constructor(private userClient: UserClient, private router: Router, private snackbarService: SnackbarService) {}

  title = 'Register';
  buttonLabel = 'Sign Up';

  onFormSubmitted(authData: { userName: string; password: string }): void {
    const isEmail = /\S+@\S+\.\S+/.test(authData.userName);
    const isPhone = /^\d+$/.test(authData.userName); // todo regex to validate phone number for countries

    if (!isEmail && !isPhone) {
      this.snackbarService.showError('A valid email address or phone number is required to register');
      return;
    }

    const user: User = {
      firstName: '',
      lastName: '',
      email: isEmail ? authData.userName : '',
      phone: isPhone ? authData.userName : '',
      password: authData.password,
      userName: authData.userName
    };

    this.userClient.register(user).subscribe({
      next: () => this.router.navigate([RoutesConstants.home])
    });
  }

  handleToggle(isRegister: boolean): void {
    if (!isRegister) {
      this.router.navigate([RoutesConstants.login]);
    }
  }
}
