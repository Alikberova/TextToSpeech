import { Injectable } from '@angular/core';
import { ConfigConstants } from '../../constants/config-constants'

@Injectable({
  providedIn: 'root',
})

export class AuthService {
  setToken(token: string): void {
    localStorage.setItem(ConfigConstants.AccessToken, token);
  }

  getToken(): string | null {
    return localStorage.getItem(ConfigConstants.AccessToken);
  }

  isAuthenticated(): boolean {
    if (this.getToken())
    {
      return true;
    }

    return false;
  }

  removeToken(): void {
    localStorage.removeItem(ConfigConstants.AccessToken);
  }
}