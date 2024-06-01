import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config-service';
import { NarakeetVoice } from '../models/dto/narakeet-voice';

@Injectable({
  providedIn: 'root',
})
export class VoiceClient {
  private configService = inject(ConfigService);
  
  private apiUrl = `${this.configService.apiUrl}/voices`;

  constructor(private http: HttpClient) {}

  getVoices(apiName: string): Observable<NarakeetVoice[]> {
    return this.http.get<NarakeetVoice[]>(`${this.apiUrl}/${apiName}`);
  }
}