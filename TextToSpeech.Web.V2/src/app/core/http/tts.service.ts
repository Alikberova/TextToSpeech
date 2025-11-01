import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SpeechResponseFormat, TtsRequest } from '../../dto/tts-request';
import { SPEECH_BASE, SPEECH_SAMPLE, AUDIO_DOWNLOAD } from './endpoints';

/**
 * TTS API client.
 * - Sample: POST `/speech/sample` with JSON body (no file) → Blob (audio/*)
 * - Full:   POST `/speech` as multipart/form-data with file → string (FileId)
 */
@Injectable({ providedIn: 'root' })
export class TtsService {
  private readonly http = inject(HttpClient);

  getSpeechSample(request: TtsRequest): Observable<Blob> {
    const options: Record<string, unknown> = {};
    options['voice'] = request.ttsRequestOptions.voice;
    if (request.ttsRequestOptions.model) {
      options['model'] = request.ttsRequestOptions.model;
    }
    // Force MP3 for sample regardless of UI choice
    options['responseFormat'] = SpeechResponseFormat.MP3;
    options['speed'] = request.ttsRequestOptions.speed;

    const payload: Record<string, unknown> = {};
    payload['ttsApi'] = request.ttsApi;
    payload['languageCode'] = request.languageCode;
    payload['input'] = request.input;
    payload['ttsRequestOptions'] = options;

    return this.http.post(SPEECH_SAMPLE, payload, { responseType: 'blob' });
  }

  createSpeech(request: TtsRequest): Observable<string> {
    if (!request.file) {
      throw new Error('File is required for full speech generation');
    }

    const form = new FormData();
    form.append('TtsApi', request.ttsApi);
    form.append('LanguageCode', request.languageCode);
    form.append('File', request.file);

    const opts = request.ttsRequestOptions;
    if (opts.model) form.append('TtsRequestOptions.Model', opts.model);
    form.append('TtsRequestOptions.Voice', opts.voice);
    form.append('TtsRequestOptions.Speed', String(opts.speed));
    form.append('TtsRequestOptions.ResponseFormat', String(opts.responseFormat));

    return this.http.post<string>(SPEECH_BASE, form);
  }

  downloadById(fileId: string): Observable<Blob> {
    return this.http.get(`${AUDIO_DOWNLOAD}/${fileId}`, { responseType: 'blob' });
  }
}
