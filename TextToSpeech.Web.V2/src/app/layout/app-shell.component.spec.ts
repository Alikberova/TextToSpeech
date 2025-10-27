import { TestBed } from '@angular/core/testing';
import { AppShellComponent } from './app-shell.component';
import { provideRouter } from '@angular/router';
import { TranslateModule, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { provideZonelessChangeDetection } from '@angular/core';

describe('AppShellComponent layout', () => {
  it('centers the footer container', async () => {
    await TestBed.configureTestingModule({
      imports: [
        AppShellComponent,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
          useDefaultLang: true,
        }),
      ],
      providers: [provideZonelessChangeDetection(), provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(AppShellComponent);
    // Simulate a wide viewport so max width rule applies
    const host = fixture.nativeElement as HTMLElement;
    host.style.width = '1400px';
    document.body.style.width = '1400px';
    fixture.detectChanges();

    const footerContainer = host.querySelector('.app-footer .container') as HTMLElement;
    const rect = footerContainer.getBoundingClientRect();
    const hostRect = host.getBoundingClientRect();

    const left = rect.left - hostRect.left;
    const right = hostRect.right - rect.right;
    // Centered if left and right are approximately equal
    expect(Math.abs(left - right)).toBeLessThan(2);
  });
});
