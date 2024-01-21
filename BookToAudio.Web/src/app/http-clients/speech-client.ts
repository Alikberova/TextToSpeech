import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConfigConstants } from '../constants/config-constants';
import { SpeechRequest } from '../models/text-to-speech';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SpeechClient {
  private apiUrl = `${ConfigConstants.ApiUrl}/speech`;

  constructor(private http: HttpClient) {}

  createSpeech(request: SpeechRequest): Observable<string> {
    if (!request.input && !request.file) {
      throw new Error(
        'Either string input or file is required to create speech'
      );
    }

    const formData = new FormData();

    formData.append('model', request.model!);

    if (request.input) {
      formData.append('input', request.input);
    }

    if (request.file) {
      formData.append('file', request.file);
    }

    if (request.input) {
      formData.append('voice', request.voice!);
    }

    if (request.input) {
      formData.append('speed', request.speed!.toString());
    }

    return this.http.post<string>(`${this.apiUrl}`, formData);
  }
}
