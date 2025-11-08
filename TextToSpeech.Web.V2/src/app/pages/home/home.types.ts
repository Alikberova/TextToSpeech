// Shared types for HomePage and related helpers
// Keep strictly typed and reusable across tests.

export type AudioStatus = 'Idle' | 'Created' | 'Processing' | 'Completed' | 'Failed' | 'Canceled';

export interface SelectOption {
  key: string;
  label: string;
}

