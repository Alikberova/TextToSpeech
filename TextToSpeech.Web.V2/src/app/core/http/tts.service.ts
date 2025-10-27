import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TtsRequest } from '../../dto/tts-request';

/**
 * TTS API client.
 * - Sample: POST `/speech/sample` with JSON body (no file) → Blob (audio/*)
 * - Full:   POST `/speech` as multipart/form-data with file → string (FileId)
 */
@Injectable({ providedIn: 'root' })
export class TtsService {
  private readonly http = inject(HttpClient);

  /** Request and receive a short audio sample for the given parameters (no file). */
  getSpeechSample(request: TtsRequest): Observable<Blob> {
    const { ttsApi, languageCode, model, speed, voice, input } = request;
    const payload = { ttsApi, languageCode, model, speed, voice, input };
    return this.http.post('/speech/sample', payload, { responseType: 'blob' });
  }

  /** Start full audio generation; returns backend FileId (guid as string). */
  createSpeech(request: TtsRequest): Observable<string> {
    if (!request.file) {
      throw new Error('File is required for full speech generation');
    }

    const form = new FormData();
    form.append('ttsApi', request.ttsApi);
    form.append('languageCode', request.languageCode);
    form.append('file', request.file);
    if (request.model) form.append('model', request.model);
    if (request.voice) form.append('voice', request.voice);
    if (request.speed != null) form.append('speed', String(request.speed));
    if (request.input) form.append('input', request.input);

    return this.http.post('/speech', form, { responseType: 'text' });
  }

  /** Download generated audio by FileId as Blob for custom handling. */
  downloadById(fileId: string): Observable<Blob> {
    return this.http.get(`/speech/${encodeURIComponent(fileId)}`, { responseType: 'blob' });
  }
}
