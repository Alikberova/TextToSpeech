import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { SpeechRequest } from '../models/text-to-speech';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config-service';

@Injectable({
  providedIn: 'root',
})
export class SpeechClient {
  private configService = inject(ConfigService);
  
  private apiUrl = `${this.configService.apiUrl}/speech`;

  constructor(private http: HttpClient) {}

  createSpeech(request: SpeechRequest): Observable<string> {
    if (!request.file) {
      throw new Error('File is required to create speech');
    }

    const formData = new FormData();
    formData.append('ttsApi', request.ttsApi!);
    formData.append("languageCode", request.languageCode);
    formData.append('file', request.file);
    if (request.model) {
      formData.append('model', request.model);
    }
    if (request.voice) {
      formData.append('voice', request.voice);
    }
    if (request.speed) {
      formData.append('speed', request.speed!.toString());
    }
    
    return this.http.post<string>(`${this.apiUrl}`, formData);
  }

  getSpeechSample(request: SpeechRequest): Observable<Blob> { //todo validate fields
    return this.http.post(`${this.apiUrl}/sample`, request, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      responseType: 'blob'
    })
  }
}
