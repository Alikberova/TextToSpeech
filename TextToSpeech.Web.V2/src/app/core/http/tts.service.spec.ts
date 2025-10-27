import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TtsService } from './tts.service';
import { provideZonelessChangeDetection } from '@angular/core';

describe('TtsService', () => {
  let service: TtsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(TtsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('posts sample request to /speech/sample and expects blob', () => {
    const reqBody = { ttsApi: 'openai', languageCode: 'en', model: 'tts-1', speed: 1.2, voice: 'alloy', input: 'Hello' };
    let received: Blob | null = null;
    service.getSpeechSample(reqBody).subscribe(b => received = b);

    const r = http.expectOne('/speech/sample');
    expect(r.request.method).toBe('POST');
    expect(r.request.responseType).toBe('blob');
    expect(r.request.body).toEqual(reqBody);
    r.flush(new Blob([new Uint8Array([1, 2, 3])], { type: 'audio/mpeg' }));
    expect(received).toBeTruthy();
  });

  it('throws if createSpeech called without file', () => {
    expect(() => service.createSpeech({ ttsApi: 'openai', languageCode: 'en' })).toThrow();
  });

  it('posts multipart to /speech and returns FileId as text', () => {
    const file = new File(['x'], 'x.txt', { type: 'text/plain' });
    let id = '';
    service.createSpeech({ ttsApi: 'openai', languageCode: 'en', voice: 'alloy', model: 'tts-1', speed: 1, file }).subscribe(v => id = v);
    const r = http.expectOne('/speech');
    expect(r.request.method).toBe('POST');
    // FormData check: browser serializes multipart; we can assert it is FormData by existence of get
    expect(r.request.body instanceof FormData).toBeTrue();
    r.flush('abc-123', { status: 200, statusText: 'OK' });
    expect(id).toBe('abc-123');
  });

  it('downloads by id as blob', () => {
    let blob: Blob | null = null;
    service.downloadById('abc').subscribe(b => blob = b);
    const r = http.expectOne('/speech/abc');
    expect(r.request.method).toBe('GET');
    expect(r.request.responseType).toBe('blob');
    r.flush(new Blob([new Uint8Array([1])], { type: 'audio/mpeg' }));
    expect(blob).toBeTruthy();
  });
});

