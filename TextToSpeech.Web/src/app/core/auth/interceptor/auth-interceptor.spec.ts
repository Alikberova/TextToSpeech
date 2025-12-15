import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, it, expect, vi } from 'vitest';
import { of } from 'rxjs';
import { API_URL } from '../../../constants/tokens';
import { AUTH_GUEST } from '../../http/endpoints';
import { GuestTokenService } from '../guest/guest-token.service';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  const apiUrl = 'https://api.example';
  let interceptor: HttpInterceptorFn;
  let tokenService: { getToken: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    tokenService = { getToken: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        { provide: API_URL, useValue: apiUrl },
        { provide: GuestTokenService, useValue: tokenService },
      ],
    });

    interceptor = (req, next) => TestBed.runInInjectionContext(() => authInterceptor(req, next));
  });

  it('skips requests that are outside the API base url', () => {
    const request = new HttpRequest('GET', 'https://other.example/data');
    const next = vi.fn((req) => of(req));

    interceptor(request, next).subscribe();

    expect(next).toHaveBeenCalledWith(request);
    expect(tokenService.getToken).not.toHaveBeenCalled();
  });

  it('adds bearer header for API requests when a token is available', () => {
    tokenService.getToken.mockReturnValue('abc123');
    const request = new HttpRequest('GET', `${apiUrl}/items`);
    const next = vi.fn((req) => of(req));

    interceptor(request, next).subscribe();

    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.get('Authorization')).toBe('Bearer abc123');
  });

  it('does not attach auth header when targeting the guest endpoint', () => {
    tokenService.getToken.mockReturnValue('abc123');
    const request = new HttpRequest('GET', `${apiUrl}${AUTH_GUEST}`);
    const next = vi.fn((req) => of(req));

    interceptor(request, next).subscribe();

    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.has('Authorization')).toBe(false);
  });
});
