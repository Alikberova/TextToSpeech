import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import {
  HttpRequest,
  HttpHandlerFn,
  HttpResponse,
  HttpEvent,
} from '@angular/common/http';
import { of } from 'rxjs';
import { baseUrlInterceptor } from './base-url-interceptor';
import { API_BASE_URL } from '../../../constants/tokens';

describe('baseUrlInterceptor', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: API_BASE_URL, useValue: 'https://api.example.com' },
      ],
    });
  });

  it('should prefix relative URLs with the base apiUrl', (done) => {
    const interceptorFn = baseUrlInterceptor;

    const req = new HttpRequest('GET', '/foo');
    const next: HttpHandlerFn = (r: HttpRequest<unknown>) =>
      of(new HttpResponse({ status: 200, url: r.url }));

    TestBed.runInInjectionContext(() => {
      interceptorFn(req, next).subscribe((event: HttpEvent<unknown>) => {
        if (event instanceof HttpResponse) {
          expect(event.url).toBe('https://api.example.com/foo');
          done();
        }
      });
    });
  });

  it('should not modify absolute URLs', (done) => {
    const interceptorFn = baseUrlInterceptor;

    const req = new HttpRequest('GET', 'https://other.host/bar');
    const next: HttpHandlerFn = (r: HttpRequest<unknown>) =>
      of(new HttpResponse({ status: 200, url: r.url }));

    TestBed.runInInjectionContext(() => {
      interceptorFn(req, next).subscribe((event: HttpEvent<unknown>) => {
        if (event instanceof HttpResponse) {
          expect(event.url).toBe('https://other.host/bar');
          done();
        }
      });
    });
  });
});
