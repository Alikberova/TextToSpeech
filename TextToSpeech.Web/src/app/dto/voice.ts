export type VoiceQualityTier = 'Standard' | 'Premium';

export interface Language {
  name: string;
  languageCode: string;
}

export interface Voice {
  name: string;
  providerVoiceId: string;
  language?: Language | null;
  qualityTier?: VoiceQualityTier;
}
