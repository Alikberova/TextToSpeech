export interface Voice {
  name: string;
  providerVoiceId: string;
  language?: Language | null;
  qualityTier?: VoiceQualityTier;
}

interface Language {
  name: string;
  languageCode: string;
}

type VoiceQualityTier = 'Standard' | 'Premium';
