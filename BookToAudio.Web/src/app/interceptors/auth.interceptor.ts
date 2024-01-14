import { HttpInterceptorFn } from '@angular/common/http';
import { ConfigConstants } from '../constants/config-constants';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(ConfigConstants.AccessToken);
  if (token)
  {
    const clonedRequest = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    return next(clonedRequest)
  }
  return next(req);
};
