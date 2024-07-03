import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SpeechRequest } from '../models/dto/text-to-speech';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config.service';
import { ApiService } from './base-client';

@Injectable({
  providedIn: 'root',
})
export class SpeechClient {
  
  private apiUrl = `${this.configService.apiUrl}/speech`;

  constructor(private apiService: ApiService, private configService: ConfigService) {}

  createSpeech(request: SpeechRequest): Observable<string> {
    if (!request.file) {
      throw new Error('File is required to create speech');
    }

    const formData = new FormData();
    formData.append('ttsApi', request.ttsApi!);
    formData.append('languageCode', request.languageCode);
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

    return this.apiService.postFormData<string>(this.apiUrl, formData);
  }

  getSpeechSample(request: SpeechRequest): Observable<Blob> {
    if (!request.input ||
        !request.languageCode ||
        !request.speed ||
        !request.ttsApi ||
        !request.voice) {
      throw new Error('Some data in SpeechRequest is missed');
    }

    return this.apiService.post(`${this.apiUrl}/sample`, request, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      responseType: 'blob'
    });
  }
}
