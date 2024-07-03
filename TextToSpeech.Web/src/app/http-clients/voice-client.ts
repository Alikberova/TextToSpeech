import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../services/config.service';
import { NarakeetVoice } from '../models/dto/narakeet-voice';
import { ApiService } from './base-client';

@Injectable({
  providedIn: 'root',
})
export class VoiceClient {
  private apiUrl = `${this.configService.apiUrl}/voices`;

  constructor(private apiService: ApiService, private configService: ConfigService) {}

  getVoices(apiName: string): Observable<NarakeetVoice[]> {
    return this.apiService.get<NarakeetVoice[]>(`${this.apiUrl}/${apiName}`);
  }
}