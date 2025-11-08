import { By } from '@angular/platform-browser';
import { createHomeFixture } from './home.page.spec-setup';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OverlayContainer } from '@angular/cdk/overlay';
import { HttpTestingController, TestRequest } from '@angular/common/http/testing';
import { TranslateService } from '@ngx-translate/core';
import { PROVIDERS, ProviderKey } from '../../../constants/tts-constants';
import { HomePage } from '../home.page';

describe('HomePage - Language dropdown behavior (Narakeet only)', () => {

  it('renders i18n keys for language options', async () => {
    const { fixture, component, httpController, overlayContainer } = await createHomeFixture();

    const optionTexts = act(component, httpController, fixture, overlayContainer);

    // With TranslateFakeLoader, translation returns the i18n key, not actual localized text
    expect(optionTexts.some(t => t.includes('languages.en-US'))).toBeTrue();
    expect(optionTexts.some(t => t.includes('languages.el-GR'))).toBeTrue();
  });

  it('renders translated language labels when UI language is Ukrainian', async () => {
    const { fixture, component, httpController, overlayContainer } = await createHomeFixture();
    const translate = TestBed.inject(TranslateService);
    translate.setTranslation('uk', {
      languages: {
        'en-US': 'Англійська (американська)',
        'el-GR': 'Грецька'
      },
      home: { language: { placeholder: 'Оберіть мову', label: 'Мова' } }
    }, true);
    translate.use('uk');

    const optionTexts = act(component, httpController, fixture, overlayContainer);

    expect(optionTexts.some(t => t.includes('Грецька'))).toBeTrue();
    expect(optionTexts.some(t => t.includes('Англійська (американська)'))).toBeTrue();
  });
});

function act(component: HomePage, httpController: HttpTestingController, fixture: ComponentFixture<HomePage>,
  overlayContainer: OverlayContainer): string[] {

  const narakeetKey = (PROVIDERS.find(p => p.key === 'narakeet') || PROVIDERS[0]).key as ProviderKey;
  component.onProviderChange(narakeetKey);
  flushNarakeetVoices(httpController);
  fixture.detectChanges();

  // Open the language select to render options into the overlay
  const languageSelectDebug = fixture.debugElement.query(By.css('mat-select[name="language"]'));
  (languageSelectDebug.nativeElement as HTMLElement).click();
  fixture.detectChanges();

  const container: HTMLElement = overlayContainer.getContainerElement();
  const optionTexts = Array.from(container.querySelectorAll('mat-option'))
    .map(n => (n.textContent || '').trim())
    .filter(Boolean);

  return optionTexts;
}

function flushNarakeetVoices(httpController: HttpTestingController): void {
  const request: TestRequest = httpController.expectOne(r => r.url.endsWith('/voices/narakeet'));
  const voices = [
    { name: 'Oliver', language: 'English (American)', languageCode: 'en-US', styles: [] },
    { name: 'eleni', language: 'Greek', languageCode: 'el-GR', styles: [] }
  ];
  request.flush(voices);
}