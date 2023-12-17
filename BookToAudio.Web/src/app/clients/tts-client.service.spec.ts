import { TestBed } from '@angular/core/testing';

import { TtsClientService } from './tts-client.service';

describe('TtsClientService', () => {
  let service: TtsClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TtsClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
