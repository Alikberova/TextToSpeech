import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, it, expect, vi } from 'vitest';
import { firstValueFrom, of } from 'rxjs';
import { API_URL } from '../../../constants/tokens';
import { GuestTokenService } from '../guest/guest-token.service';
import { authInterceptor } from './auth-interceptor';
import { AUTH_GUEST } from '../../http/endpoints';

describe('authInterceptor', () => {
  const apiUrl = 'https://api.example';
  let interceptor: HttpInterceptorFn;

  let tokenService: {
    ensureValidToken: ReturnType<typeof vi.fn>;
    refreshToken: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    tokenService = { ensureValidToken: vi.fn(), refreshToken: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        { provide: API_URL, useValue: apiUrl },
        { provide: GuestTokenService, useValue: tokenService },
      ],
    });

    interceptor = (req, next) => TestBed.runInInjectionContext(() => authInterceptor(req, next));
  });

  it('skips requests that are outside the API base url', async () => {
    const request = new HttpRequest('GET', 'https://other.example/data');
    const next = vi.fn((req) => of(req));

    await firstValueFrom(interceptor(request, next));

    expect(next).toHaveBeenCalledWith(request);
    expect(tokenService.ensureValidToken).not.toHaveBeenCalled();
    expect(tokenService.refreshToken).not.toHaveBeenCalled();
  });

  it('adds bearer header for API requests using a token from GuestTokenService', async () => {
    tokenService.ensureValidToken.mockResolvedValue('abc123');

    const request = new HttpRequest('GET', `${apiUrl}/items`);
    const next = vi.fn((req) => of(req));

    await firstValueFrom(interceptor(request, next));

    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.get('Authorization')).toBe('Bearer abc123');
  });

  it('does not attach auth header when targeting the guest endpoint', async () => {
    tokenService.ensureValidToken.mockResolvedValue('abc123');

    const request = new HttpRequest('GET', `${apiUrl}${AUTH_GUEST}`);
    const next = vi.fn((req) => of(req));

    await firstValueFrom(interceptor(request, next));

    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.has('Authorization')).toBe(false);
    expect(tokenService.ensureValidToken).not.toHaveBeenCalled();
    expect(tokenService.refreshToken).not.toHaveBeenCalled();
  });
});
