import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { environment } from '../environments/environment.development';
import { API_BASE_URL } from './constants/tokens';
import { baseUrlInterceptor } from './core/http/base-url/base-url-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    { provide: API_BASE_URL, useValue: environment.apiBase },
    provideHttpClient(withInterceptors([baseUrlInterceptor])),
    // Configure ngx-translate to load JSON files from /i18n/* via the public folder.
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'en',
        loader: {
          provide: TranslateLoader,
          useFactory: jsonLoaderFactory,
          deps: [HttpClient],
        },
        useDefaultLang: true,
      }),
    ),
  ]
};

// Minimal JSON loader for ngx-translate using HttpClient
class JsonTranslateLoader implements TranslateLoader {
  constructor(private http: HttpClient, private prefix = '/i18n/', private suffix = '.json') {}
  getTranslation(lang: string) {
    return this.http.get<Record<string, unknown>>(`${this.prefix}${lang}${this.suffix}`);
  }
}

export function jsonLoaderFactory(http: HttpClient) {
  return new JsonTranslateLoader(http);
}
