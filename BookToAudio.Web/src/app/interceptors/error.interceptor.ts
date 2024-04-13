import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';
import { SnackbarService } from '../shared-ui/snackbar-service';
import { inject } from '@angular/core';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const logger = inject(NGXLogger);
  const snackbarService = inject(SnackbarService);
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      logger.error(error);
      snackbarService.showError("Oops. Unknown error occurred", undefined, 6000)
      return throwError(() => new Error('Unhandled exception ocurred'));
    })
  );
};