export const OPEN_AI_KEY = 'openai';
export const NARAKEET_KEY = 'narakeet';

// TtsApis
export const PROVIDERS = [
    { key: OPEN_AI_KEY, label: 'OpenAI' },
    { key: NARAKEET_KEY, label: 'Narakeet' },
] as const;

export type ProviderKey = typeof PROVIDERS[number]['key'];

export const PROVIDER_MODELS: Record<ProviderKey, string[] | null> = {
  narakeet: null,
  openai: ['tts-1', 'gpt-4o-mini-tts'],
} as const;

export const OPEN_AI_VOICES = [
    { key: 'alloy', label: 'Alloy' },
    { key: 'ash', label: 'Ash' },
    { key: 'ballad', label: 'Ballad' },
    { key: 'coral', label: 'Coral' },
    { key: 'echo', label: 'Echo' },
    { key: 'fable', label: 'Fable' },
    { key: 'onyx', label: 'Onyx' },
    { key: 'nova', label: 'Nova' },
    { key: 'sage', label: 'Sage' },
    { key: 'shimmer', label: 'Shimmer' },
    { key: 'verse', label: 'Verse' },
] as const;

export const DEMO_TEXT = 'Welcome to our voice showcase! Listen as we bring words to life, demonstrating a range of unique and dynamic vocal styles!';
export const ACCEPTABLE_FILE_TYPES = ['.txt', '.pdf', '.epub'];
export const MAX_INPUT_LENGTH = 10000000; //todo MaxSize dictionary for every type?; todo length check on backend
