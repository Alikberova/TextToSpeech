import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TtsFormComponent } from './tts-form.component';

describe('TtsFormComponent', () => {
  let component: TtsFormComponent;
  let fixture: ComponentFixture<TtsFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TtsFormComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TtsFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
