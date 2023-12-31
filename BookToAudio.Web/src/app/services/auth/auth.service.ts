import { Injectable } from '@angular/core';
import { Сonstants } from '../../constants'

@Injectable({
  providedIn: 'root',
})

export class AuthService {
  setToken(token: string): void {
    localStorage.setItem(Сonstants.AccessToken, token);
  }

  getToken(): string | null {
    return localStorage.getItem(Сonstants.AccessToken);
  }

  isAuthenticated(): boolean {
    if (this.getToken())
    {
      return true;
    }

    return false;
  }
}