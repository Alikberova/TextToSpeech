import { createHomeFixture, SELECTOR_LANGUAGE_SELECT, LANG_I18N_PREFIX, LANGUAGE_CODE_EN_US, LANGUAGE_CODE_EL_GR, LOCALE_UK, flushNarakeetVoices, openMatSelect, getOverlayOptionTexts } from './home.page.spec-setup';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OverlayContainer } from '@angular/cdk/overlay';
import { HttpTestingController } from '@angular/common/http/testing';
import { TranslateService } from '@ngx-translate/core';
import { NARAKEET_KEY, PROVIDERS, ProviderKey } from '../../../constants/tts-constants';
import { HomePage } from '../home.page';

describe('HomePage - Language dropdown behavior (Narakeet only)', () => {

  it('renders i18n keys for language options', async () => {
    const { fixture, component, httpController, overlayContainer } = await createHomeFixture();

    const optionTexts = act(component, httpController, fixture, overlayContainer);

    // With TranslateFakeLoader, translation returns the i18n key, not actual localized text
    expect(optionTexts.some(t => t.includes(LANG_I18N_PREFIX + LANGUAGE_CODE_EN_US))).toBeTrue();
    expect(optionTexts.some(t => t.includes(LANG_I18N_PREFIX + LANGUAGE_CODE_EL_GR))).toBeTrue();
  });

  it('renders translated language labels when UI language is Ukrainian', async () => {
    const { fixture, component, httpController, overlayContainer } = await createHomeFixture();
    const translate = TestBed.inject(TranslateService);
    translate.setTranslation(LOCALE_UK, {
      languages: {
        [LANGUAGE_CODE_EN_US]: 'Англійська (американська)',
        [LANGUAGE_CODE_EL_GR]: 'Грецька'
      },
      home: { language: { placeholder: 'Оберіть мову', label: 'Мова' } }
    }, true);
    translate.use(LOCALE_UK);

    const optionTexts = act(component, httpController, fixture, overlayContainer);

    expect(optionTexts.some(t => t.includes('Грецька'))).toBeTrue();
    expect(optionTexts.some(t => t.includes('Англійська (американська)'))).toBeTrue();
  });
});

function act(component: HomePage, httpController: HttpTestingController, fixture: ComponentFixture<HomePage>,
  overlayContainer: OverlayContainer): string[] {

  const narakeetKey = (PROVIDERS.find(p => p.key === NARAKEET_KEY))!.key as ProviderKey;
  component.onProviderChange(narakeetKey);
  flushNarakeetVoices(httpController);
  fixture.detectChanges();

  // Open the language select to render options into the overlay
  openMatSelect(fixture, SELECTOR_LANGUAGE_SELECT);

  return getOverlayOptionTexts(overlayContainer);
}
