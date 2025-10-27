import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { I18nService } from './i18n.service';
import { TranslateFakeLoader, TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';

describe('I18nService', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
          useDefaultLang: true,
        }),
      ],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
  });

  it('defaults to en when storage empty', () => {
    spyOn(localStorage, 'getItem').and.returnValue(null);
    const svc = TestBed.inject(I18nService);
    const translate = TestBed.inject(TranslateService);
    expect(svc.current()).toBe('en');
    expect(translate.currentLang).toBe('en');
  });

  it('initializes from localStorage when present (uk)', () => {
    spyOn(localStorage, 'getItem').and.returnValue('uk');
    const svc = TestBed.inject(I18nService);
    const translate = TestBed.inject(TranslateService);
    expect(svc.current()).toBe('uk');
    expect(translate.currentLang).toBe('uk');
  });

  it('toggle switches language and stores to localStorage', () => {
    spyOn(localStorage, 'getItem').and.returnValue(null);
    const setSpy = spyOn(localStorage, 'setItem');
    const svc = TestBed.inject(I18nService);

    expect(svc.current()).toBe('en');
    svc.toggle();
    expect(svc.current()).toBe('uk');
    expect(setSpy).toHaveBeenCalledWith('lang', 'uk');

    svc.toggle();
    expect(svc.current()).toBe('en');
    expect(setSpy).toHaveBeenCalledWith('lang', 'en');
  });

  it('set ignores unsupported languages', () => {
    spyOn(localStorage, 'getItem').and.returnValue(null);
    const setSpy = spyOn(localStorage, 'setItem');
    const translate = TestBed.inject(TranslateService);
    const svc = TestBed.inject(I18nService);

    svc.set('en');
    expect(svc.current()).toBe('en');
    expect(translate.currentLang).toBe('en');

    // @ts-expect-error testing invalid set
    svc.set('de');
    expect(svc.current()).toBe('en');
    expect(setSpy).not.toHaveBeenCalledWith('lang', 'de');
  });
});
