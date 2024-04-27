import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class ValidationService {
  static matchRequiredPassword(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    let errMessage = "";    

    if (/(\s)/.test(password)) {
      errMessage += "The password must not contain spaces.\n"
    }
    if (!/.{6}/.test(password)) {
      errMessage += "The password must be at least 6 characters long.\n"
    }
    if (!/([A-Z])/.test(password)) {
      errMessage += "The password must contain at least one capital letter.\n"
    }
    if (!/([\W_])/.test(password)) {
      errMessage += "The password must contain at least one special character.\n"
    }
    
    if (errMessage !== "") {
      return { invalid: { message: errMessage } }
    }
    
    return null;
  }
}
