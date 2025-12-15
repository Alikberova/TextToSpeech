import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, afterEach, it, expect } from 'vitest';
import { API_URL, GUEST_JWT_KEY } from '../../../constants/tokens';
import { AUTH_GUEST } from '../../http/endpoints';
import { GuestTokenService } from './guest-token.service';

describe('GuestTokenService', () => {
  let service: GuestTokenService;
  let http: HttpTestingController;
  const apiUrl = 'https://api.example';

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClientTesting(), { provide: API_URL, useValue: apiUrl }],
    });
    service = TestBed.inject(GuestTokenService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('returns cached token without writing to session storage', () => {
    service.setToken('cached', false);

    expect(service.getToken()).toBe('cached');
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBeNull();
  });

  it('reads token from session storage when nothing is cached', () => {
    sessionStorage.setItem(GUEST_JWT_KEY, 'from-session');

    expect(service.getToken()).toBe('from-session');
    sessionStorage.removeItem(GUEST_JWT_KEY);
    expect(service.getToken()).toBe('from-session');
  });

  it('fetches and persists a token when missing', async () => {
    const tokenPromise = service.ensureToken(true);

    const req = http.expectOne(`${apiUrl}${AUTH_GUEST}`);
    expect(req.request.method).toBe('POST');
    req.flush({ accessToken: 'api-token', expiresAtUtc: 'now' });

    const token = await tokenPromise;

    expect(token).toBe('api-token');
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBe('api-token');
    expect(service.getToken()).toBe('api-token');
  });

  it('clears cached and persisted tokens', () => {
    service.setToken('to-clear', true);

    service.clearToken();

    expect(service.getToken()).toBeNull();
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBeNull();
  });
});
