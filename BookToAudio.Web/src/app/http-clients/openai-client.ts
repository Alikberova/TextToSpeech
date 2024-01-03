import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Constants } from '../constants';
import { TextToSpeech } from '../models/text-to-speech';

@Injectable({
  providedIn: 'root',
})
export class OpenaiClient {
  private apiUrl = Constants.ApiOpenAiUrl;
  private apiKey = '';
  //todo найти способ как взять апикей с переменых окружения
  constructor(private http: HttpClient) {}

  createSpeech(textToSpeech: TextToSpeech) {
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
