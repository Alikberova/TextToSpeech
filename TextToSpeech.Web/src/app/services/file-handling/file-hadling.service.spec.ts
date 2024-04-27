import { TestBed } from '@angular/core/testing';

import { FileHadlingService } from './file-hadling.service';

describe('FileHadlingService', () => {
  let service: FileHadlingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FileHadlingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
