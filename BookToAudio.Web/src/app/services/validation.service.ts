import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class ValidationService {
 
  static validationEmail(control: AbstractControl): ValidationErrors | null {

    const email = control.value;
    const emailRegexp = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    
    if (email !== "" && !emailRegexp.test(email)) {
      return { invalid: true };
    }
    return null;
  }

  static matchRequiredPassword(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    const passwordRegexp = /^(?=.*[A-Z])(?=.*[\W_])(?!.*\s).{6,}$/;
    
    if (!passwordRegexp.test(password)) {
      return { invalid: { message: "The password has no capital letters and has no special char" } };
    }
    else {
      return null;
    }
  }
}
