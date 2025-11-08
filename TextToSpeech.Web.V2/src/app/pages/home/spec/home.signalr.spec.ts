import { By } from '@angular/platform-browser';
import { createHomeFixture, fillValidFormForOpenAI, DOWNLOAD_BUTTON_SELECTOR, SUBMIT_BUTTON_SELECTOR, PROGRESS_PROCESSING_VALUE, PROGRESS_COMPLETE_VALUE, PROGRESS_VALID_VALUE } from './home.page.spec-setup';
import { SPEECH_BASE } from '../../../core/http/endpoints';
import { SignalRService } from '../../../core/realtime/signalr.service';

describe('HomePage - SignalR and Progress', () => {
  it('enables Download button when SignalR reports Completed', async () => {
    const signalRStub = new SignalRStub();

    const { fixture, component, httpController } = await createHomeFixture([
      { provide: SignalRService, useValue: signalRStub },
    ]);

    fillValidFormForOpenAI(component);
    fixture.detectChanges();

    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();
    const testRequest = httpController.expectOne(SPEECH_BASE);
    const formdata = testRequest.request.body as FormData;
    expect(formdata.get('TtsRequestOptions.ResponseFormat')).toBe('mp3');
    testRequest.flush('id-1');

    signalRStub.trigger('id-1', 'Processing', PROGRESS_PROCESSING_VALUE);
    fixture.detectChanges();
    expect(component.status()).toBe('Processing');
    signalRStub.trigger('id-1', 'Completed', PROGRESS_COMPLETE_VALUE);
    fixture.detectChanges();
    expect(component.status()).toBe('Completed');
    const downloadButton = fixture.debugElement.query(By.css(DOWNLOAD_BUTTON_SELECTOR));
    expect(downloadButton.properties['disabled']).toBeFalse();
  });

  it('updates progress when valid and ignores invalid values', async () => {
    const stub = new SignalRStub();

    const { fixture, component, httpController } = await createHomeFixture([
      { provide: SignalRService, useValue: stub },
    ]);

    fillValidFormForOpenAI(component);
    fixture.detectChanges();

    fixture.debugElement.query(By.css(SUBMIT_BUTTON_SELECTOR)).nativeElement.click();
    fixture.detectChanges();
    const testRequest = httpController.expectOne(SPEECH_BASE);
    testRequest.flush('file-xyz');

    stub.trigger('file-xyz', 'Processing', PROGRESS_VALID_VALUE);
    fixture.detectChanges();
    expect(component.progress()).toBe(PROGRESS_VALID_VALUE);

    stub.trigger('file-xyz', 'Processing', Number.NaN);
    fixture.detectChanges();
    expect(component.progress()).toBe(PROGRESS_VALID_VALUE);
  });
});

class SignalRStub {
  private callback?: (id: string, status: string, progress: number, error?: string) => void;
  startConnection() { return; }
  addAudioStatusListener(cb: (id: string, s: string, p: number, e?: string) => void) { this.callback = cb; }
  cancelProcessing() { return; }
  trigger(id: string, s: string, p: number, e?: string) { this.callback?.(id, s, p, e); }
}