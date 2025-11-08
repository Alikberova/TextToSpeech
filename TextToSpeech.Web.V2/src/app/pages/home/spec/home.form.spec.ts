import { By } from '@angular/platform-browser';
import { createHomeFixture, SUBMIT_BUTTON_SELECTOR, DEFAULT_FILE_CONTENT, DEFAULT_FILE_NAME, providerKeyWithModel } from './home.page.spec-setup';
import { HttpTestingController } from '@angular/common/http/testing';
import { OPEN_AI_VOICES } from '../../../constants/tts-constants';
import { SPEECH_BASE } from '../../../core/http/endpoints';
import { SpeechResponseFormat } from '../../../dto/tts-request';
import { ComponentFixture } from '@angular/core/testing';
import { HomePage } from '../home.page';
import { OverlayContainer } from '@angular/cdk/overlay';

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
    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    component.voice.set(OPEN_AI_VOICES[0].key);
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    component.responseFormat.set(SpeechResponseFormat.WAV);
    fixture.detectChanges();

    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();

    const testRequest = httpController.expectOne(SPEECH_BASE);
    const formData = testRequest.request.body as FormData;
    expect(formData.get('TtsRequestOptions.ResponseFormat')).toBe('wav');
  });

  it('clear button resets all the fields to default', async () => {
    component.provider.set('openai');
    component.voice.set('alloy');
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    component.onSampleTextInput('custom');
    fixture.detectChanges();

    fixture.debugElement.query(By.css('button[data-testid="clearBtn"]')).nativeElement.click();
    fixture.detectChanges();

    expect(component.provider()).toBe('');
    expect(component.model()).toBe('');
    expect(component.language()).toBe('');
    expect(component.voice()).toBe('');
    expect(component.file()).toBeNull();
    expect(component.sampleText()).toBe('home.sample.defaultText');
    expect(component.status()).toBe('Idle');
    expect(component.progress()).toBe(0);
  });

  it('labels exist and are visible; placeholders present', async () => {
    const element = fixture.nativeElement as HTMLElement;
    // Form-field labels
    const labels = Array.from(element.querySelectorAll('mat-form-field mat-label')).map(n => n.textContent?.trim() ?? '');
    expect(labels.join(' ')).toContain('home.provider.label');
    // Language label is conditional (Narakeet only); not asserting here
    expect(labels.join(' ')).toContain('home.voice.label');
    // Upload label associated via for attribute
    const uploadLabel = element.querySelector('label.upload-label') as HTMLLabelElement;
    const input = element.querySelector('input[type="file"]') as HTMLInputElement;
    expect(uploadLabel.getAttribute('for')).toBe(input.id);
    // Voice placeholder visible in trigger
    // Placeholder is shown via trigger in template; here we assert no selected label yet
    // and control is disabled to indicate initial placeholder state.
    const voiceSelect = element.querySelector('mat-select[name="voice"]');
    expect(voiceSelect?.getAttribute('aria-disabled')).toBe('true');

    // Provider placeholder exists as first disabled option in the panel
    const providerSelectDebug = fixture.debugElement.query(By.css('mat-select[name="provider"]'));
    (providerSelectDebug.nativeElement as HTMLElement).click();
    fixture.detectChanges();
    const container: HTMLElement = overlayContainer.getContainerElement();
    const providerOptions = Array.from(container.querySelectorAll('mat-option'))
      .map(n => (n.textContent || '').trim());
    expect(providerOptions.some(t => t.includes('home.provider.placeholder'))).toBeTrue();
  });
});
