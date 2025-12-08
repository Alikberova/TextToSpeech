export const OPEN_AI_KEY = 'openai';
export const NARAKEET_KEY = 'narakeet';
export const ACCEPTABLE_FILE_TYPES = ['.txt', '.pdf', '.epub'];
export const MAX_INPUT_LENGTH = 10000000; //todo MaxSize dictionary for every type?; todo length check on backend

export const PROVIDERS = [
    { key: OPEN_AI_KEY, label: 'OpenAI' },
    { key: NARAKEET_KEY, label: 'Narakeet' },
] as const;

export const PROVIDER_MODELS: Record<ProviderKey, string[] | null> = {
  narakeet: null,
  openai: ['tts-1', 'gpt-4o-mini-tts'],
} as const;

export type ProviderKey = typeof PROVIDERS[number]['key'];
