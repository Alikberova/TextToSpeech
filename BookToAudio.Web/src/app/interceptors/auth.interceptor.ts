import { HttpInterceptorFn } from '@angular/common/http';
import { Сonstants } from '../constants';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(Сonstants.AccessToken);
  if (token)
  {
    const clonedRequest = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    return next(clonedRequest)
  }
  return next(req);
};
