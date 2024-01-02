import { HttpInterceptorFn } from '@angular/common/http';
import { Constants } from '../constants';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(Constants.AccessToken);
  if (token)
  {
    const clonedRequest = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    return next(clonedRequest)
  }
  return next(req);
};
