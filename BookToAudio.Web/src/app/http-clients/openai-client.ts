import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConfigConstants } from '../constants/config-constants';
import { SpeechRequest } from '../models/text-to-speech';

@Injectable({
  providedIn: 'root',
})
export class OpenaiClient {
  private apiUrl = ConfigConstants.ApiOpenAiUrl;
  private apiKey = '';
  //todo найти способ как взять апикей с переменых окружения
  constructor(private http: HttpClient) {}

  createSpeech(textToSpeech: SpeechRequest) {
    const body = {
      model: textToSpeech.model,
      voice: textToSpeech.voice,
      input: textToSpeech.input,
      speed: textToSpeech.speed,
    };
    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.apiKey}`,
      'Content-Type': 'application/json',
    });
    
    return this.http.post<Blob>(this.apiUrl, body, {
      headers: headers,
      responseType: 'blob' as 'json',
    });
  }
}
