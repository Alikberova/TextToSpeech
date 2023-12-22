import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OpenaiService {
  private baseUrl = 'http://localhost:3000'; // Update with your server's URL

  constructor(private http: HttpClient) {}

  generateSpeech(modelConfig: any): Observable<any> {
    const url = `${this.baseUrl}/generate-speech`;
    return this.http.post(url, { modelConfig });
  }
}
