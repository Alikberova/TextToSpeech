import { APP_INITIALIZER, ApplicationConfig, importProvidersFrom } from '@angular/core';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';
import { ConfigService } from './services/config-service';

export const appConfig: ApplicationConfig = {
  providers: [ 
    importProvidersFrom(BrowserAnimationsModule),
    provideRouter(routes),
    provideHttpClient(withInterceptors([ authInterceptor ])),
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      multi: true,
      deps: [ConfigService],
    },
  ],
};

export function initConfig(appConfig: ConfigService) {
  return () => appConfig.setConfig();
}