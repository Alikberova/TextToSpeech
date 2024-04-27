export interface SpeechRequest {
  ttsApi: string;
  model: string;
  speed?: number;
  voice?: string;
  input?: string;
  file?: File;
}
