import { ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NgModel } from '@angular/forms';
import { createHomeFixture, providerKeyWithModel, providerKeyWithoutModel, SUBMIT_BUTTON_SELECTOR, DEFAULT_FILE_CONTENT, DEFAULT_FILE_NAME } from './home.page.spec-setup';
import { HttpTestingController } from '@angular/common/http/testing';
import { OPEN_AI_VOICES, PROVIDERS, PROVIDER_MODELS, ProviderKey } from '../../../constants/tts-constants';
import { HomePage } from '../home.page';

describe('HomePage - Validation UX', () => {
  let fixture: ComponentFixture<HomePage>;
  let component: HomePage;
  let httpController: HttpTestingController;

  beforeEach(async () => {
    const created = await createHomeFixture();
    fixture = created.fixture;
    component = created.component;
    httpController = created.httpController;
  });

  function getMatErrors(f: ComponentFixture<HomePage>) {
    return f.debugElement.queryAll(By.css('mat-error'));
  }

  it('shows no errors on initial load', async () => {
    expect(getMatErrors(fixture).length).toBe(0);
    expect(fixture.debugElement.query(By.css('.form-error'))).toBeNull();
  });

  it('shows a field error after it is touched', async () => {
    const providerNgModel = fixture.debugElement.queryAll(By.directive(NgModel))
      .map(el => el.injector.get(NgModel))
      .find((m: NgModel) => m.name === 'provider') as NgModel;
    providerNgModel.control.markAsTouched();
    fixture.detectChanges();
    expect(getMatErrors(fixture).length).toBe(1);
  });

  it('after a failed submit, invalid fields display errors and form-level error appears', async () => {
    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();
    expect(getMatErrors(fixture).length).toBeGreaterThanOrEqual(2);
    expect(fixture.debugElement.query(By.css('.form-error'))).not.toBeNull();
  });

  it('a field error disappears once it becomes valid', async () => {
    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();
    expect(getMatErrors(fixture).length).toBeGreaterThan(0);

    const providerWithoutModel = providerKeyWithoutModel();
    component.onProviderChange(providerWithoutModel);
    const pendingVoices = httpController.match(req => req.url.endsWith('/voices/narakeet'));
    pendingVoices.forEach(req => req.flush([]));
    fixture.detectChanges();

    expect(getMatErrors(fixture).length).toBeLessThan(4);
  });

  it('form-level error hides after all fields become valid', async () => {
    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('.form-error'))).not.toBeNull();

    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    component.voice.set('alloy');
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.form-error'))).toBeNull();
  });

  it('focusFirstInvalid focuses Voice when provider requires model but model is auto-selected', async () => {
    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    component.voice.set('');
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    fixture.detectChanges();

    const getRef = (name: 'modelEl' | 'providerEl' | 'voiceEl') =>
      (component as unknown as Record<string, unknown>)[name] as { focus: () => void };
    const modelRef = getRef('modelEl');
    const providerRef = getRef('providerEl');
    const voiceRef = getRef('voiceEl');
    spyOn(modelRef, 'focus').and.callThrough();
    spyOn(providerRef, 'focus').and.callThrough();
    spyOn(voiceRef, 'focus').and.callThrough();

    component.submit();
    await fixture.whenStable();

    expect(voiceRef.focus).toHaveBeenCalled();
    expect(providerRef.focus).not.toHaveBeenCalled();
    expect(modelRef.focus).not.toHaveBeenCalled();
  });

  it('focusFirstInvalid focuses Voice when missing in Narakeet flow', async () => {
    const narakeetKey = (PROVIDERS.find(p => PROVIDER_MODELS[p.key] === null) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(narakeetKey);
    const pending = httpController.match(r => r.url.endsWith('/voices/narakeet'));
    pending.forEach(req => req.flush([{ name: 'v1', language: 'English', languageCode: 'en-US', styles: [] }]));
    component.onLanguageChange('en-US');
    component.voice.set('');
    component.file.set(new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME));
    fixture.detectChanges();

    const voiceRef = (component as unknown as Record<string, unknown>)['voiceEl'] as { focus: () => void };
    const providerRef = (component as unknown as Record<string, unknown>)['providerEl'] as { focus: () => void };
    spyOn(voiceRef, 'focus').and.callThrough();
    spyOn(providerRef, 'focus').and.callThrough();

    component.submit();
    await fixture.whenStable();

    expect(voiceRef.focus).toHaveBeenCalled();
    expect(providerRef.focus).not.toHaveBeenCalled();
  });

  it('requires model only when provider needs it and selects first automatically', async () => {

    // Provider that requires model -> model auto-selected; no model error after submit
    const providerWithModel = providerKeyWithModel();
    component.onProviderChange(providerWithModel);
    // language not required for OpenAI
    const firstVoiceKey = OPEN_AI_VOICES[0].key;
    component.voice.set(firstVoiceKey);
    fixture.detectChanges();
    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();

    // With provider requiring model, requiresModel() is true and model field is present & valid
    expect(component.requiresModel()).toBeTrue();
    const formFields = fixture.debugElement.queryAll(By.css('mat-form-field.field'));
    const modelFieldEl = formFields.find(fields => !!fields.query(By.css('mat-select[name="model"]')));
    expect(modelFieldEl).toBeTruthy();
    expect(modelFieldEl!.attributes['aria-invalid']).not.toBe('true');

    // Switch to provider that does NOT require model -> model field disappears
    const providerWithoutModel = (PROVIDERS.find(p => PROVIDER_MODELS[p.key] === null) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithoutModel);
    fixture.detectChanges();

    const formFields2 = fixture.debugElement.queryAll(By.css('mat-form-field.field'));
    const modelFieldGone = formFields2.find(ff => !!ff.query(By.css('mat-select[name="model"]')));
    expect(component.requiresModel()).toBeFalse();
    expect(modelFieldGone).toBeUndefined();
  });

});
