import { By } from '@angular/platform-browser';
import { createHomeFixture, DEFAULT_FILE_CONTENT, DEFAULT_FILE_NAME, DEFAULT_OPENAI_VOICE_KEY, DEFAULT_SAMPLE_I18N_KEY, SELECTOR_VOICE_SELECT, ATTR_ARIA_DISABLED, SELECTOR_PROVIDER_SELECT, SELECTOR_LABELS, SELECTOR_CLEAR_BUTTON, I18N_HOME_PROVIDER_LABEL, I18N_HOME_VOICE_LABEL, I18N_HOME_PROVIDER_PLACEHOLDER, clickSubmit, selectOpenAiMinimal, openMatSelect, getOverlayOptionTexts, expectOneEndsWith } from './home.page.spec-setup';
import { HttpTestingController } from '@angular/common/http/testing';
import { SPEECH_BASE } from '../../../core/http/endpoints';
import { SpeechResponseFormat } from '../../../dto/tts-request';
import { ComponentFixture } from '@angular/core/testing';
import { HomePage } from '../home.page';
import { OverlayContainer } from '@angular/cdk/overlay';
import { AUDIO_STATUS } from '../home.types';

describe('HomePage - Form submission', () => {
  let fixture: ComponentFixture<HomePage>;
  let component: HomePage;
  let overlayContainer: OverlayContainer;
  let httpController: HttpTestingController;

  beforeEach(async () => {
    const created = await createHomeFixture();
    fixture = created.fixture;
    component = created.component;
    overlayContainer = created.overlayContainer;
    httpController = created.httpController;
  });

  it('includes selected response format in full speech request', async () => {
    selectOpenAiMinimal(component);
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    component.responseFormat.set(SpeechResponseFormat.WAV);
    fixture.detectChanges();

    clickSubmit(fixture);

    const testRequest = expectOneEndsWith(httpController, SPEECH_BASE);
    const formData = testRequest.request.body as FormData;
    expect(formData.get('TtsRequestOptions.ResponseFormat')).toBe('wav');
  });

  it('clear button resets all the fields to default', async () => {
    component.provider.set('openai');
    component.voice.set(DEFAULT_OPENAI_VOICE_KEY);
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    component.onSampleTextInput('custom');
    fixture.detectChanges();

    fixture.debugElement.query(By.css(SELECTOR_CLEAR_BUTTON)).nativeElement.click();
    fixture.detectChanges();

    expect(component.provider()).toBe('');
    expect(component.model()).toBe('');
    expect(component.language()).toBe('');
    expect(component.voice()).toBe('');
    expect(component.file()).toBeNull();
    expect(component.sampleText()).toBe(DEFAULT_SAMPLE_I18N_KEY);
    expect(component.status()).toBe(AUDIO_STATUS.Idle);
    expect(component.progress()).toBe(0);
  });

  it('labels exist and are visible; placeholders present', async () => {
    const element = fixture.nativeElement as HTMLElement;
    // Form-field labels
    const labels = Array.from(element.querySelectorAll(SELECTOR_LABELS)).map(n => n.textContent?.trim() ?? '');
    expect(labels.join(' ')).toContain(I18N_HOME_PROVIDER_LABEL);
    // Language label is conditional (Narakeet only); not asserting here
    expect(labels.join(' ')).toContain(I18N_HOME_VOICE_LABEL);
    // Upload label associated via for attribute
    const uploadLabel = element.querySelector('label.upload-label') as HTMLLabelElement;
    const input = element.querySelector('input[type="file"]') as HTMLInputElement;
    expect(uploadLabel.getAttribute('for')).toBe(input.id);
    // Voice placeholder visible in trigger
    // Placeholder is shown via trigger in template; here we assert no selected label yet
    // and control is disabled to indicate initial placeholder state.
    const voiceSelect = element.querySelector(SELECTOR_VOICE_SELECT);
    expect(voiceSelect?.getAttribute(ATTR_ARIA_DISABLED)).toBe('true');

    // Provider placeholder exists as first disabled option in the panel
    openMatSelect(fixture, SELECTOR_PROVIDER_SELECT);
    const providerOptions = getOverlayOptionTexts(overlayContainer);
    expect(providerOptions.some(t => t.includes(I18N_HOME_PROVIDER_PLACEHOLDER))).toBeTrue();
  });
});
