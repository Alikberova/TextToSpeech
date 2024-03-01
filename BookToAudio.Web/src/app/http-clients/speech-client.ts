import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ConfigConstants } from '../constants/config-constants';
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
    formData.append('model', request.model!);
    if (request.input) {
      formData.append('input', request.input);
    }
    if (request.file) {
      formData.append('file', request.file);
    }
    if (request.voice !== undefined) {
      formData.append('voice', request.voice.toString());
    }
    if (request.speed) {
      formData.append('speed', request.speed!.toString());
    }
    
    return this.http.post<string>(`${this.apiUrl}`, formData);
  }

  getSpeechSample(request: SpeechRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/sample`, request, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      responseType: 'blob'
    })
  }
}
