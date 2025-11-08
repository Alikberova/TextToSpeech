import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OverlayContainer } from '@angular/cdk/overlay';
import { API_URL, SERVER_URL } from '../../../constants/tokens';
import { ProviderKey, PROVIDERS, PROVIDER_MODELS, OPEN_AI_VOICES } from '../../../constants/tts-constants';
import { HomePage } from '../home.page';

export const SUBMIT_BUTTON_SELECTOR = 'button[type="submit"]';
export const DOWNLOAD_BUTTON_SELECTOR = 'button[mat-stroked-button]';

export const DEFAULT_FILE_CONTENT = 'qwerty';
export const DEFAULT_FILE_NAME = 'any.txt';

export const PROGRESS_PROCESSING_VALUE = 40;
export const PROGRESS_VALID_VALUE = 55;
export const PROGRESS_COMPLETE_VALUE = 100;

export function getBaseProviders(): unknown[] {
  return [
    provideZonelessChangeDetection(),
    provideHttpClient(),
    provideHttpClientTesting(),
    { provide: API_URL, useValue: 'https://fake-localhost:1234/api' },
    { provide: SERVER_URL, useValue: 'https://fake-localhost:1234' },
  ];
}

export async function createHomeFixture(extraProviders: unknown[] = []) {
  await TestBed.resetTestingModule().configureTestingModule({
    imports: [
      HomePage,
      TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }, useDefaultLang: true }),
    ],
    providers: [...getBaseProviders(), ...extraProviders],
  }).compileComponents();

  const fixture = TestBed.createComponent(HomePage);
  const component = fixture.componentInstance;
  const httpController = TestBed.inject(HttpTestingController);
  const overlayContainer = TestBed.inject(OverlayContainer);
  fixture.detectChanges();
  return { fixture, component, httpController, overlayContainer } as const;
}

export function providerKeyWithModel(): ProviderKey {
  return (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
}

export function providerKeyWithoutModel(): ProviderKey {
  return (PROVIDERS.find(p => PROVIDER_MODELS[p.key] === null) || PROVIDERS[0]).key as ProviderKey;
}

export function fillValidFormForOpenAI(component: HomePage): void {
  const provider = providerKeyWithModel();
  component.onProviderChange(provider);
  component.voice.set(OPEN_AI_VOICES[0].key);
  component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
}
