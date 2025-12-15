import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { API_URL } from '../../../constants/tokens';
import { AUTH_GUEST } from '../../http/endpoints';
import { GuestTokenService } from '../guest/guest-token.service';

const RETRY_HEADER = 'x-auth-retry';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(GuestTokenService);
  const apiUrl = inject(API_URL);

  if (!req.url.startsWith(apiUrl) || req.url.endsWith(AUTH_GUEST)) {
    return next(req);
  }

  const persistInSession = true;

  return from(tokenService.ensureValidToken(persistInSession)).pipe(
    switchMap((token) => {
      const authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
      });

      return next(authReq);
    }),
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401) {
        return throwError(() => err);
      }

      if (req.headers.has(RETRY_HEADER)) {
        return throwError(() => err);
      }

      return from(tokenService.refreshToken(persistInSession)).pipe(
        switchMap((newToken) => {
          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`,
              [RETRY_HEADER]: '1',
            },
          });

          return next(retryReq);
        }),
        catchError((refreshErr) => throwError(() => refreshErr)),
      );
    }),
  );
};
