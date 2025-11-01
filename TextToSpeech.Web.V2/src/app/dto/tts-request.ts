export enum SpeechResponseFormat {
  MP3 = 'mp3',
  Opus = 'opus',
  AAC = 'aac',
  Flac = 'flac',
  WAV = 'wav',
  PCM = 'pcm',
}

export interface TtsRequestOptions {
  model?: string;
  voice: string;
  speed?: number;
  responseFormat?: SpeechResponseFormat;
}

export interface TtsRequest {
  ttsApi: string;
  languageCode: string;
  file?: File;
  input?: string;
  ttsRequestOptions: TtsRequestOptions;
}
