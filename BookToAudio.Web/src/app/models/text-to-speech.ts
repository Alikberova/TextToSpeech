import { SpeechVoice } from "./speech-voice.enum";

export interface SpeechRequest {
  model: string;
  speed?: number;
  voice?: SpeechVoice;
  input?: string;
  file?: File;
}
