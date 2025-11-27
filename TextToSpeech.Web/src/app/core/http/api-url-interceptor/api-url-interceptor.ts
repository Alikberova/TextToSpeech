import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_URL } from '../../../constants/tokens';

export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const apiUrl = inject(API_URL);

  if (!apiUrl) {
    return next(req);
  }

  const isStatic =
    req.url.startsWith('/assets/') ||
    req.url.startsWith('assets/') ||
    req.url.startsWith('/i18n/') ||
    req.url.startsWith('i18n/');

  if (isStatic) {
    return next(req);
  }

  if (/^https?:\/\//i.test(req.url)) {
    return next(req);
  }

  const url = req.url.startsWith('/') ? `${apiUrl}${req.url}` : `${apiUrl}/${req.url}`;
  return next(req.clone({ url }));
};
