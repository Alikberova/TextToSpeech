import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})

export class SnackbarService {
  constructor(private snackBar: MatSnackBar) {}

  showError(error: string, action?: string | undefined, duration?: number) {
    console.error(error)
    const config = {
      panelClass: ['snackbar', 'snackbar-error'],
      duration: duration
    }
    this.show(error, config, action);
  }

  showSuccess(message: string, action?: string | undefined, duration?: number) {
    const config = {
      panelClass: ['snackbar', 'snackbar-success'],
      duration: duration
    }
    this.show(message, config, action);
  }
  
  private show(message: string, config: MatSnackBarConfig, action?: string | undefined) {
    action ??= 'Close';
    config.duration ??= 8000;
    this.snackBar.open(message, action, config!);
  }
}
