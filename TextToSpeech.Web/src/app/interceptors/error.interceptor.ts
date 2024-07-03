import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';
import { SnackbarService } from '../ui-services/snackbar-service';
import { inject } from '@angular/core';
import { SomethingWentWrong } from '../constants/content/errors';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const logger = inject(NGXLogger);
  const snackbarService = inject(SnackbarService);
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      logger.error(error);
      snackbarService.showError(SomethingWentWrong)
      return throwError(() => new Error('Unhandled exception ocurred'));
    })
  );
};