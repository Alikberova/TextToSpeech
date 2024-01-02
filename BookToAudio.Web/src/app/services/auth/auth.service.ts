import { Injectable } from '@angular/core';
import { Constants } from '../../constants'

@Injectable({
  providedIn: 'root',
})

export class AuthService {
  setToken(token: string): void {
    localStorage.setItem(Constants.AccessToken, token);
  }

  getToken(): string | null {
    return localStorage.getItem(Constants.AccessToken);
  }

  isAuthenticated(): boolean {
    if (this.getToken())
    {
      return true;
    }

    return false;
  }

  removeToken(): void {
    localStorage.removeItem(Constants.AccessToken);
  }
}