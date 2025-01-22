export interface SpeechRequest {
  ttsApi: string;
  languageCode: string;
  model?: string;
  speed?: number;
  voice?: string;
  input?: string;
  file?: File;
}
