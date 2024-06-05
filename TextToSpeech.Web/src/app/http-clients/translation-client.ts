import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config-service';
import { TranslationRequest } from '../models/dto/translation-request';

@Injectable({
  providedIn: 'root',
})
export class TranslationClient {
  private configService = inject(ConfigService);
  
  private apiUrl = `${this.configService.apiUrl}/translation`;

  constructor(private http: HttpClient) {}

  translate(request: TranslationRequest): Observable<string> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Cache-Control': 'public, max-age=1209600'
    });
    return this.http.post<string>(`${this.apiUrl}`,
      request,
      {
        headers,
        responseType: 'text' as 'json'
      }
    )
  }
}
