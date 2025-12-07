import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { VOICES } from '../endpoints';
import type { Voice } from '../../../dto/voice';

@Injectable({ providedIn: 'root' })
export class VoiceService {
  private readonly http = inject(HttpClient);

  getVoices(provider: string): Observable<Voice[]> {
    const params = new HttpParams().set('provider', provider);
    return this.http.get<Voice[]>(VOICES, { params });
  }
}
