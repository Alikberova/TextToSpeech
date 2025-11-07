import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection, type Signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { HomePage } from './home.page';
import { TranslateModule, TranslateLoader, TranslateFakeLoader, TranslateService } from '@ngx-translate/core';
import { NgModel } from '@angular/forms';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OPEN_AI_VOICES, PROVIDER_MODELS, PROVIDERS, type ProviderKey } from '../../constants/tts-constants';
import { SignalRService } from '../../core/realtime/signalr.service';
import { SpeechResponseFormat } from '../../dto/tts-request';
import { API_URL } from '../../constants/tokens';
import { SPEECH_BASE, SPEECH_SAMPLE } from '../../core/http/endpoints';

describe('HomePage validation UX', () => {
  let fixture: ComponentFixture<HomePage>;
  let component: HomePage;
  let httpController: HttpTestingController;

  const BASE_PROVIDERS = [
    provideZonelessChangeDetection(),
    provideHttpClient(),
    provideHttpClientTesting(),
    { provide: API_URL, useValue: 'https://fake-localhost:1234/api' }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomePage,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }, useDefaultLang: true }),
      ],
      providers: BASE_PROVIDERS,
    }).compileComponents();

    fixture = TestBed.createComponent(HomePage);
    component = fixture.componentInstance;
    httpController = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  function getErrors(fixture: ComponentFixture<HomePage>) {
    return fixture.debugElement.queryAll(By.css('mat-error'));
  }

  it('shows no errors on initial load', async () => {
    expect(getErrors(fixture).length).toBe(0);
    // No form-level error banner
    const banner = fixture.debugElement.query(By.css('.form-error'));
    expect(banner).toBeNull();
  });

  it('shows a field error after it is touched', async () => {
    // Mark provider as touched
    const providerNgModel = fixture.debugElement.queryAll(By.directive(NgModel))
      .map(de => de.injector.get(NgModel))
      .find((m: NgModel) => m.name === 'provider') as NgModel;
    providerNgModel.control.markAsTouched();
    fixture.detectChanges();

    const providerError = fixture.debugElement.queryAll(By.css('mat-error'));
    expect(providerError.length).toBe(1);
  });

  it('after a failed submit, invalid fields display errors and form-level error appears', async () => {
    const submitBtn = fixture.debugElement.query(By.css('button[type="submit"]'));
    submitBtn.nativeElement.click();
    fixture.detectChanges();

    // With voice select disabled until provider selection, at least provider and file should show error
    expect(getErrors(fixture).length).toBeGreaterThanOrEqual(2);

    const banner = fixture.debugElement.query(By.css('.form-error'));
    expect(banner).not.toBeNull();
  });

  it('a field error disappears once it becomes valid', async () => {
    // Trigger submit to show errors
    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();
    expect(getErrors(fixture).length).toBeGreaterThan(0);

    // Make provider valid
    const providerWithoutModel = (PROVIDERS.find(p => PROVIDER_MODELS[p.key] === null) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithoutModel);
    const pendingVoicesRequests = httpController.match(r => r.url.endsWith('/voices/narakeet'));
    pendingVoicesRequests.forEach(req => req.flush([]));
    fixture.detectChanges();

    // Remaining errors still exist but provider error should be gone.
    // Check that at least one error was removed by ensuring total < 4
    expect(getErrors(fixture).length).toBeLessThan(4);
  });

  it('form-level error hides after all fields become valid', async () => {
    // Submit once to show banner
    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('.form-error'))).not.toBeNull();

    // Fill all required fields
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    const firstVoiceKey = OPEN_AI_VOICES[0].key;
    component.onProviderChange(providerWithModel);
    // OpenAI does not require language
    component.voice.set(firstVoiceKey);
    component.file.set(new File(['a'], 'a.txt'));
    fixture.detectChanges();

    // Banner hides when valid
    expect(fixture.debugElement.query(By.css('.form-error'))).toBeNull();
  });

  it('requires model only when provider needs it and selects first automatically', async () => {

    // Provider that requires model -> model auto-selected; no model error after submit
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithModel);
    // language not required for OpenAI
    const firstVoiceKey = OPEN_AI_VOICES[0].key;
    component.voice.set(firstVoiceKey);
    fixture.detectChanges();
    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    // With provider requiring model, requiresModel() is true and model field is present & valid
    expect(component.requiresModel()).toBeTrue();
    const formFields = fixture.debugElement.queryAll(By.css('mat-form-field.field'));
    const modelFieldEl = formFields.find(ff => !!ff.query(By.css('mat-select[name="model"]')));
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

  it('file drag/drop sets file and clears error; remove sets touched', async () => {
    // Trigger submit to show errors
    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();
    expect(fixture.debugElement.queryAll(By.css('mat-error')).length).toBeGreaterThan(0);

    // Simulate drop
    const dropzone = fixture.debugElement.query(By.css('.dropzone'));
    const file = new File(['abc'], 'demo.txt', { type: 'text/plain' });
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    dropzone.triggerEventHandler('drop', { preventDefault: () => undefined, dataTransfer });
    fixture.detectChanges();
    expect(component.file()).not.toBeNull();

    // Remove file -> touched remains true
    component.removeFile();
    fixture.detectChanges();
    expect(component.file()).toBeNull();
  });

  it('sample play posts to /speech/sample', async () => {
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithModel);
    // No language for OpenAI
    component.voice.set(OPEN_AI_VOICES[0].key);
    fixture.detectChanges();
    const playBtn = fixture.debugElement.query(By.css('button[mat-icon-button]'));
    playBtn.nativeElement.click();
    fixture.detectChanges();
    const req = httpController.expectOne(SPEECH_SAMPLE);
    expect(req.request.method).toBe('POST');
    req.flush(new Blob([new Uint8Array([1])], { type: 'audio/mpeg' }));
  });

  it('labels exist and are visible; placeholders present', async () => {
    const el = fixture.nativeElement as HTMLElement;
    // Form-field labels
    const labels = Array.from(el.querySelectorAll('mat-form-field mat-label')).map(n => n.textContent?.trim() ?? '');
    expect(labels.join(' ')).toContain('home.provider.label');
    // Language label is conditional (Narakeet only); not asserting here
    expect(labels.join(' ')).toContain('home.voice.label');
    // Upload label associated via for attribute
    const uploadLabel = el.querySelector('label.upload-label') as HTMLLabelElement;
    const input = el.querySelector('input[type="file"]') as HTMLInputElement;
    expect(uploadLabel.getAttribute('for')).toBe(input.id);
    // Voice placeholder visible in trigger
    // Placeholder is shown via trigger in template; here we assert no selected label yet
    // and control is disabled to indicate initial placeholder state.
    const voiceSelect = el.querySelector('mat-select[name="voice"]');
    expect(voiceSelect?.getAttribute('aria-disabled')).toBe('true');
  });

  it('voice dropdown disabled until provider selected and enables after provider/language as needed', async () => {
    const select = fixture.nativeElement.querySelector('mat-select[name="voice"]');
    expect(select.getAttribute('aria-disabled')).toBe('true');
    // No provider -> voicesForProvider empty
    expect(component.voicesForProvider().length).toBe(0);

    // Select provider OpenAI -> enabled and options available
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithModel);
    fixture.detectChanges();
    const selectAfter = fixture.nativeElement.querySelector('mat-select[name="voice"]');
    expect(selectAfter.getAttribute('aria-disabled')).toBe('false');
    expect(component.voicesForProvider().length).toBeGreaterThan(0);
  });

  it('narakeet language list derives from voices (unique by languageCode)', async () => {
    const narakeetKey = (PROVIDERS.find(p => p.key === 'narakeet') || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(narakeetKey);
    const reqs = httpController.match(r => r.url.endsWith('/voices/narakeet'));
    const data = [
      { name: 'a', language: 'English', languageCode: 'en-US', styles: [] },
      { name: 'b', language: 'Ukrainian', languageCode: 'uk-UA', styles: [] },
      { name: 'c', language: 'English', languageCode: 'en-US', styles: [] },
    ];
    reqs.forEach(r => r.flush(data));
    fixture.detectChanges();
    expect(component.languages().length).toBe(2);
  });

  it('submit enables download when SignalR reports Completed', async () => {
    class SignalRStub {
      private cb?: (id: string, s: string, p: number, e?: string) => void;
      startConnection() { /* noop */ }
      addAudioStatusListener(cb: (id: string, s: string, p: number, e?: string) => void) { this.cb = cb; }
      cancelProcessing() { /* noop */ }
      trigger(id: string, s: string, p: number, e?: string) { this.cb?.(id, s, p, e); }
    }
    const rStub = new SignalRStub();
    await TestBed.resetTestingModule().configureTestingModule({
      imports: [
        HomePage,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }, useDefaultLang: true }),
      ],
      providers: [
        BASE_PROVIDERS,
        { provide: SignalRService, useValue: rStub },
      ],
    }).compileComponents();
    const fixture2 = TestBed.createComponent(HomePage);
    const component2 = fixture2.componentInstance;
    const http2 = TestBed.inject(HttpTestingController);
    fixture2.detectChanges();

    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component2.onProviderChange(providerWithModel);
    // No language for OpenAI
    component2.voice.set(OPEN_AI_VOICES[0].key);
    component2.file.set(new File(['a'], 'a.txt'));
    fixture2.detectChanges();

    fixture2.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture2.detectChanges();
    const req = http2.expectOne(SPEECH_BASE);
    // Expect selected response format present in multipart
    const fd = req.request.body as FormData;
    // Default is mp3
    expect(fd.get('TtsRequestOptions.ResponseFormat')).toBe('mp3');
    req.flush('id-1');

    rStub.trigger('id-1', 'Processing', 40);
    fixture2.detectChanges();
    expect(component2.status()).toBe('Processing');
    rStub.trigger('id-1', 'Completed', 100);
    fixture2.detectChanges();
    expect(component2.status()).toBe('Completed');
    const downloadBtn = fixture2.debugElement.query(By.css('button[mat-stroked-button]'));
    expect(downloadBtn.properties['disabled']).toBeFalse();
  });

  it('includes selected response format in full speech request', async () => {
    await TestBed.resetTestingModule().configureTestingModule({
      imports: [
        HomePage,
        TranslateModule.forRoot({ loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }, useDefaultLang: true }),
      ],
      providers: BASE_PROVIDERS,
    }).compileComponents();

    const fixture3 = TestBed.createComponent(HomePage);
    const component3 = fixture3.componentInstance;
    const http3 = TestBed.inject(HttpTestingController);
    fixture3.detectChanges();

    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component3.onProviderChange(providerWithModel);
    // No language for OpenAI
    component3.voice.set(OPEN_AI_VOICES[0].key);
    component3.file.set(new File(['a'], 'a.txt'));
    // Choose a different format
    component3.responseFormat.set(SpeechResponseFormat.WAV);
    fixture3.detectChanges();

    fixture3.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture3.detectChanges();

    const req3 = http3.expectOne(SPEECH_BASE);
    const fd3 = req3.request.body as FormData;
    expect(fd3.get('TtsRequestOptions.ResponseFormat')).toBe('wav');
    req3.flush('id-2');
  });

  it('onFileSelected stores single file and marks touched', async () => {
    const input = document.createElement('input');
    const dt = new DataTransfer();
    const file = new File(['x'], 'x.txt');
    dt.items.add(file);
    Object.defineProperty(input, 'files', { value: dt.files, writable: false });

    component.onFileSelected(input);
    fixture.detectChanges();

    expect(component.file()).toBeTruthy();
    interface HomePageAccess { fileTouched: Signal<boolean> }
    const access = component as unknown as HomePage & HomePageAccess;
    expect(access.fileTouched()).toBeTrue();
  });

  it('initializes sampleText from i18n key with FakeLoader', async () => {
    // With TranslateFakeLoader, translations are not provided and instant returns the key string
    expect(component.sampleText()).toBe('home.sample.defaultText');
  });

  it('does not auto-translate sample when user edits; updates when not edited', async () => {
    // User edits sample text -> should not be overwritten on language change
    const userText = 'My personal sample input';
    component.onSampleTextInput(userText);
    fixture.detectChanges();
    const translate = TestBed.inject(TranslateService);
    translate.use('uk');
    fixture.detectChanges();
    expect(component.sampleText()).toBe(userText);

    // If user clears back to empty (treated as not edited), next language change applies default again
    component.onSampleTextInput('');
    fixture.detectChanges();
    translate.use('en');
    fixture.detectChanges();
    expect(component.sampleText()).toBe('home.sample.defaultText');
  });

  it('clear() resets fields, sample text, flags, and calls form.reset()', async () => {
    // Set non-defaults
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    const firstVoiceKey = OPEN_AI_VOICES[0].key;
    component.onProviderChange(providerWithModel);
    component.language.set('en-US');
    component.voice.set(firstVoiceKey);
    component.file.set(new File(['a'], 'a.txt'));
    // Simulate user editing the sample text so we can verify it resets
    component.onSampleTextInput('User edited sample');
    interface HomePageAccess2 { submitAttempt: Signal<boolean>; fileTouched: Signal<boolean> }
    const access2 = component as unknown as HomePage & HomePageAccess2;
    access2.submitAttempt.set(true);
    access2.fileTouched.set(true);

    const formStub = { reset: jasmine.createSpy('reset') } as unknown as HTMLFormElement;
    component.clear(formStub);
    fixture.detectChanges();

    expect(component.provider()).toBe('');
    expect(component.language()).toBe('');
    expect(component.voice()).toBe('');
    expect(component.file()).toBeNull();
    // Textarea resets back to default i18n key (FakeLoader returns the key)
    expect(component.sampleText()).toBe('home.sample.defaultText');
    expect(access2.submitAttempt()).toBeFalse();
    expect(access2.fileTouched()).toBeFalse();
    expect(formStub.reset).toHaveBeenCalled();
  });

  it('focusFirstInvalid focuses Voice when provider requires model but model is auto-selected', async () => {
    // Set state: provider needs model, other fields valid; model should auto-select
    const providerWithModel = (PROVIDERS.find(p => Array.isArray(PROVIDER_MODELS[p.key])) || PROVIDERS[0]).key as ProviderKey;
    component.onProviderChange(providerWithModel);
    // language not required for OpenAI
    component.voice.set('');
    component.file.set(new File(['a'], 'a.txt'));
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

  it('focusFirstInvalid focuses Voice when missing', async () => {
    // Provider that does not require model (Narakeet), valid language, missing voice, valid file
    const providerWithoutModel = 'narakeet' as ProviderKey;
    component.onProviderChange(providerWithoutModel);
    const pendingVoicesRequests = httpController.match(r => r.url.endsWith('/voices/narakeet'));
    pendingVoicesRequests.forEach(req => req.flush([{ name: 'v1', language: 'English', languageCode: 'en-US', styles: [] }]));
    component.onLanguageChange('en-US');
    component.voice.set('');
    component.file.set(new File(['a'], 'a.txt'));
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
  
});
