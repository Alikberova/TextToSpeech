import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(private http: HttpClient) {}

  post<T>(url: string, body: any, options?: {}): Observable<T> {
    return this.http.post<T>(url, body, options);
  }

  postFormData<T>(url: string, formData: FormData): Observable<T> {
    return this.http.post<T>(url, formData);
  }

  get<T>(url: string, options?: {}): Observable<T> {
    return this.http.get<T>(url, options);
  }

  delete<T>(url: string, options?: {}): Observable<T> {
    return this.http.delete<T>(url, options);
  }

  put<T>(url: string, body: any, options?: {}): Observable<T> {
    return this.http.put<T>(url, body, options);
  }
}
