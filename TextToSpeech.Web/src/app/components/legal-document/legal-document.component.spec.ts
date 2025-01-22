import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LegalDocumentComponent } from './legal-document.component';

describe('LegalDocumentComponent', () => {
  let component: LegalDocumentComponent;
  let fixture: ComponentFixture<LegalDocumentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LegalDocumentComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LegalDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
