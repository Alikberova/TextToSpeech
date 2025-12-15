import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { API_URL } from '../../../constants/tokens';
import { AUTH_GUEST } from '../../http/endpoints';
import { inject } from '@angular/core';
import { GuestTokenService } from '../guest/guest-token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(GuestTokenService);
  const apiUrl = inject(API_URL);

  if (!req.url.startsWith(apiUrl)) {
    return next(req);
  }

  if (req.url.endsWith(AUTH_GUEST)) {
    return next(req);
  }

  const token = tokenService.getToken();
  if (!token) {
    return next(req);
  }

  const authReq: HttpRequest<unknown> = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
  
  return next(authReq);
};
