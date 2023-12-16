import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class BaseHttpClientService {
  private baseUrl: string;

  constructor(private http: HttpClient, baseUrl: string) {
    this.baseUrl = baseUrl || ''; // Set a default value if necessary
  }

  get<T>(url: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${url}`);
  }
}
