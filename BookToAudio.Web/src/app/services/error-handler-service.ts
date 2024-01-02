import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { SnackbarService } from '../shared-ui/snackbar-service';

@Injectable({
  providedIn: 'root',
})

export class ErrorHandlerService {
    constructor(private snackBarService: SnackbarService) {}

    handleHttpError(error: HttpErrorResponse): void {
      console.error(error)
      if (typeof error.error === "string")
      {
        this.snackBarService.showError(error.error);
        return;
      }

      this.snackBarService.showError(error.message);
    }

}
