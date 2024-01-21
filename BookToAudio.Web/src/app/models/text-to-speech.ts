export interface SpeechRequest {
  model: string;
  speed?: number;
  voice?: string;
  input?: string;
  file?: File
}
