import { OPEN_AI_KEY, OPEN_AI_VOICES } from '../../constants/tts-constants';
import type { NarakeetVoice } from '../../dto/narakeet-voice';
import { AUDIO_STATUS, type AudioStatus, type SelectOption } from './home.types';

// Capitalize only the first letter, leave the rest intact
export function capitalizeFirstLetter(text: string): string {
  if (!text) {
    return text;
  }
  return text[0].toUpperCase() + text.slice(1);
}

// Build language select options from Narakeet voices
export function getLanguagesFromNarakeetVoices(voices: readonly NarakeetVoice[]): SelectOption[] {
  const map = new Map<string, string>();
  for (const v of voices) {
    if (v.languageCode && !map.has(v.languageCode)) {
      map.set(v.languageCode, `languages.${v.languageCode}`);
    }
  }
  return Array.from(map.entries())
    .map(([key, label]) => ({ key, label }))
    .sort((a, b) => a.label.localeCompare(b.label));
}

// Build voices options based on provider and optional language
export function getVoicesForProvider(
  providerKey: string | undefined,
  languageCode: string | undefined,
  narakeetVoices: readonly NarakeetVoice[],
): readonly SelectOption[] {
  if (!providerKey) {
    return [] as const;
  }
  if (providerKey === OPEN_AI_KEY) {
    return OPEN_AI_VOICES;
  }
  const list = narakeetVoices.filter(v => !languageCode || v.languageCode === languageCode);
  return list.map(v => ({ key: v.name, label: capitalizeFirstLetter(v.name) }));
}

// Map server status to an appropriate Material icon name
export function mapStatusToIcon(status: AudioStatus): string {
  switch (status) {
    case AUDIO_STATUS.Created:
      return 'schedule';
    case AUDIO_STATUS.Processing:
      return 'autorenew';
    case AUDIO_STATUS.Completed:
      return 'check_circle';
    case AUDIO_STATUS.Failed:
      return 'error';
    case AUDIO_STATUS.Canceled:
      return 'cancel';
    case AUDIO_STATUS.Idle:
    default:
      return 'info';
  }
}

// Build a filename for download given original name and selected format
export function buildDownloadFilename(originalName: string, extension: string): string {
  const dotIndex = originalName.lastIndexOf('.');
  const baseName = dotIndex > -1 ? originalName.substring(0, dotIndex) : originalName;
  return `${baseName}.${extension}`;
}
