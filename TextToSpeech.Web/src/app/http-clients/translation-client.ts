import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config.service';
import { TranslationRequest } from '../models/dto/translation-request';
import { ApiService } from './base-client';

@Injectable({
  providedIn: 'root',
})
export class TranslationClient {
  private apiUrl = `${this.configService.apiUrl}/translation`;

  constructor(private apiService: ApiService, private configService: ConfigService) {}

  translate(request: TranslationRequest): Observable<string> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Cache-Control': 'public, max-age=1209600' // todo check if Cache-Control is needed here
    });

    return this.apiService.post<string>(`${this.apiUrl}`, request, {
      headers,
      responseType: 'text' as 'json'
    });
  }
}
