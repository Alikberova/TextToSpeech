import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, afterEach, it, expect } from 'vitest';
import { API_URL, GUEST_JWT_EXPIRES_KEY, GUEST_JWT_KEY } from '../../../constants/tokens';
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

  it('returns token from in-memory state without persisting it', () => {
    service['state'] = {
      token: 'cached',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    };

    expect(service.getToken()).toBe('cached');
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBeNull();
  });

  it('loads token from session storage into memory on first access', () => {
    sessionStorage.setItem(GUEST_JWT_KEY, 'from-session');
    sessionStorage.setItem(GUEST_JWT_EXPIRES_KEY, new Date(Date.now() + 60_000).toISOString());

    expect(service.getToken()).toBe('from-session');

    sessionStorage.removeItem(GUEST_JWT_KEY);
    sessionStorage.removeItem(GUEST_JWT_EXPIRES_KEY);

    expect(service.getToken()).toBe('from-session');
  });

  it('requests a new guest token and persists it when none exists', async () => {
    const tokenPromise = service.ensureValidToken(true);

    const req = http.expectOne(`${apiUrl}${AUTH_GUEST}`);
    expect(req.request.method).toBe('POST');
    req.flush({
      accessToken: 'api-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    const token = await tokenPromise;

    expect(token).toBe('api-token');
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBe('api-token');
    expect(service.getToken()).toBe('api-token');
  });

  it('clears token from memory and removes persisted session values', async () => {
    const tokenPromise = service.ensureValidToken(true);

    const req = http.expectOne(`${apiUrl}${AUTH_GUEST}`);
    req.flush({
      accessToken: 'api-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    await tokenPromise;

    service.clearToken();

    expect(service.getToken()).toBeNull();
    expect(sessionStorage.getItem(GUEST_JWT_KEY)).toBeNull();
    expect(sessionStorage.getItem(GUEST_JWT_EXPIRES_KEY)).toBeNull();
  });
});
