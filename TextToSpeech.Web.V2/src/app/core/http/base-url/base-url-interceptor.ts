import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '../../../constants/tokens';

export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const base = inject(API_BASE_URL);

  if (!base) {
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

  const url = req.url.startsWith('/') ? `${base}${req.url}` : `${base}/${req.url}`;
  return next(req.clone({ url }));
};
