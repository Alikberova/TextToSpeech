import { createHomeFixture, DEFAULT_FILE_CONTENT, DEFAULT_FILE_NAME, DEFAULT_SAMPLE_I18N_KEY, LOCALE_UK, LOCALE_EN, clickPlayButton, selectOpenAiMinimal, expectOneEndsWith } from './home.page.spec-setup';
import { ComponentFixture } from '@angular/core/testing';
import { Signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { HomePage } from '../home.page';
import { By } from '@angular/platform-browser';
import { SPEECH_SAMPLE } from '../../../core/http/endpoints';
import { HttpTestingController } from '@angular/common/http/testing';

describe('HomePage - Sample text and file interactions', () => {
  let fixture: ComponentFixture<HomePage>;
  let component: HomePage;
  let httpController: HttpTestingController;

  beforeEach(async () => {
    const created = await createHomeFixture();
    fixture = created.fixture;
    component = created.component;
    httpController = created.httpController;
  });

  it('onFileSelected stores single file and marks touched', async () => {
    const input = document.createElement('input');
    const dataTransfer = new DataTransfer();
    const file = new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME);
    dataTransfer.items.add(file);
    Object.defineProperty(input, 'files', { value: dataTransfer.files, writable: false });

    component.onFileSelected(input);
    fixture.detectChanges();

    expect(component.file()).toBeTruthy();
    expectFileTouched(component);
  });

  it('file drag/drop sets file; remove sets touched', async () => {
    // Simulate drop
    const dropzone = fixture.debugElement.query(By.css('.dropzone'));
    const file = new File([DEFAULT_FILE_CONTENT], DEFAULT_FILE_NAME, { type: 'text/plain' });
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    dropzone.triggerEventHandler('drop', { preventDefault: () => undefined, dataTransfer });
    fixture.detectChanges();
    expect(component.file()).not.toBeNull();

    // Remove file -> touched remains true
    component.removeFile();
    fixture.detectChanges();
    expect(component.file()).toBeNull();
    expectFileTouched(component);
  });

  it('initializes sampleText from i18n key with FakeLoader and respects user edits on language change', async () => {
    // With TranslateFakeLoader, translations echo keys
    expect(component.sampleText()).toBe(DEFAULT_SAMPLE_I18N_KEY);

    const userText = 'My personal sample input';
    component.onSampleTextInput(userText);
    fixture.detectChanges();
    // Trigger language change and ensure it does not overwrite user edits
    const translate = fixture.debugElement.injector.get(TranslateService) as TranslateService;
    translate.use(LOCALE_UK);
    expect(component.sampleText()).toBe(userText);

    // If user clears back to empty (treated as not edited), next language change applies default again
    component.onSampleTextInput('');
    fixture.detectChanges();
    translate.use(LOCALE_EN);
    fixture.detectChanges();
    expect(component.sampleText()).toBe(DEFAULT_SAMPLE_I18N_KEY);
  });

  it('sample play posts to /speech/sample', async () => {
    selectOpenAiMinimal(component);
    fixture.detectChanges();

    clickPlayButton(fixture);

    const testRequest = expectOneEndsWith(httpController, SPEECH_SAMPLE);
    expect(testRequest.request.method).toBe('POST');
    testRequest.flush(new Blob([new Uint8Array([1])], { type: 'audio/mpeg' }));
  });

});

function expectFileTouched(component: HomePage) {
  interface HomePageAccess { fileTouched: Signal<boolean>; }
  const access = component as unknown as HomePage & HomePageAccess;
  expect(access.fileTouched()).toBeTrue();
}
