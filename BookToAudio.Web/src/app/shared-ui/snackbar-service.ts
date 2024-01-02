import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})

export class SnackbarService {
  constructor(private snackBar: MatSnackBar) {}

  show(message: string, action: string = 'Close', duration: number = 4000) {
    this.snackBar.open(message, action, {
      duration
    });
  }

  showError(error: string) {
    console.error(error)
    this.show(error);
  }
}
