import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_URL, GUEST_JWT_KEY } from '../../../constants/tokens';
import { AUTH_GUEST } from '../../http/endpoints';

@Injectable({
  providedIn: 'root',
})

export class GuestTokenService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  private token: string | null = null;

  getToken(): string | null {
    if (this.token !== null) {
      return this.token;
    }

    const fromSession = sessionStorage.getItem(GUEST_JWT_KEY);
    if (fromSession) {
      this.token = fromSession;
      return this.token;
    }

    return null;
  }

  setToken(token: string, persistInSession: boolean): void {
    this.token = token;

    if (persistInSession) {
      sessionStorage.setItem(GUEST_JWT_KEY, token);
      return;
    }

    sessionStorage.removeItem(GUEST_JWT_KEY);
  }

  clearToken(): void {
    this.token = null;
    sessionStorage.removeItem(GUEST_JWT_KEY);
  }

  async ensureToken(persistInSession = false): Promise<string> {
    const existing = this.getToken();
    if (existing) {
      return existing;
    }

    const url = `${this.apiUrl}${AUTH_GUEST}`;
    const res = await firstValueFrom(this.http.post<GuestTokenResponse>(url, {}));

    this.setToken(res.accessToken, persistInSession);
    return res.accessToken;
  }
}

interface GuestTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
};
