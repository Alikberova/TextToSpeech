import { provideZonelessChangeDetection } from '@angular/core';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';

/**
 * Shared TranslateModule configuration used across multiple specs so we keep
 * the boilerplate consistent.
 */
export function getTranslateTestingModule() {
  return TranslateModule.forRoot({
    loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
    useDefaultLang: true,
  });
}

/**
 * Returns providers array that always includes zoneless change detection plus
 * any test-specific providers.
 */
export function getZonelessProviders(additionalProviders: unknown[] = []): unknown[] {
  return [provideZonelessChangeDetection(), ...additionalProviders];
}
