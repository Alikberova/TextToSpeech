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
      debugger;
      logger.error(error);
      snackbarService.showError("Oops! Something went wobbly. We're on it!")
      return throwError(() => new Error('Unhandled exception ocurred'));
    })
  );
};