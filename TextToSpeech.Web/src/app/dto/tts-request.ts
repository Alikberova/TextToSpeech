import type { Voice } from './voice';

export interface TtsRequestOptions {
  model?: string;
  voice: Voice;
  speed: number;
  responseFormat: string;
}

export interface TtsRequest {
  ttsApi: string;
  languageCode: string;
  file?: File;
  input?: string;
  ttsRequestOptions: TtsRequestOptions;
}
