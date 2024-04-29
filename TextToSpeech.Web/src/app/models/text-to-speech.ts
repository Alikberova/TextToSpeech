export interface SpeechRequest {
  ttsApi: string;
  model: string;
  languageCode: string;
  speed?: number;
  voice?: string;
  input?: string;
  file?: File;
}
