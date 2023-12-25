import { Component } from '@angular/core';
import { AuthFormComponent } from "../auth-form/auth-form.component";

@Component({
    selector: 'app-register',
    standalone: true,
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
    imports: [AuthFormComponent]
})

export class RegisterComponent {
  title = 'Register';
  buttonLabel = 'Sign Up';

  onFormSubmitted(authData: { userName: string; password: string }): void {
    console.log("hey");
    console.log("username " + authData.userName);
    // Implement your registration logic here using authData.username and authData.password
  }
}
