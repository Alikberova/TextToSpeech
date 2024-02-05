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

    if (/(\s)/.test(password)) {
      return { invalid: { message: "The password must not contain spaces." } };
    }
    else if (!/.{6}/.test(password)) {
      return { invalid: { message: "The password must be at least 6 characters long." } }
    }
    else if (!/([A-Z])/.test(password)) {
      return { invalid: { message: "The password must contain at least one capital letter." } };
    }
    else if (!/([\W_])/.test(password)) {
      return { invalid: { message: "The password must contain at least one special character." } }
    }
    else {
      return null;
    }
  }
}
