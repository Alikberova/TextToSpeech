import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { importProvidersFrom, APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { INGXLoggerConfig, LoggerModule, NGXLogger, NgxLoggerLevel } from 'ngx-logger';
import { routes } from './app/app.routes';
import { authInterceptor } from './app/interceptors/auth.interceptor';
import { ConfigService } from './app/services/config-service';
import { errorInterceptor } from './app/interceptors/error.interceptor';

const appConfig: ApplicationConfig = {
  providers: [ 
    importProvidersFrom(BrowserAnimationsModule,
      LoggerModule.forRoot(
        getLoggerConfig('')
      )
    ),
    provideRouter(routes),
    provideHttpClient(withInterceptors([ authInterceptor, errorInterceptor ])),
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      multi: true,
      deps: [ConfigService, NGXLogger],
    },
  ],
};

function initConfig(appConfig: ConfigService, logger: NGXLogger) {
  return () => appConfig.setConfig().then(() => {
    // Update the serverLoggingUrl in the logger service configuration
    logger.updateConfig(
      getLoggerConfig(appConfig.serverLoggingUrl)
      );
  });
}

function getLoggerConfig(serverLoggingUrl: string): INGXLoggerConfig {
  return {
    serverLoggingUrl: serverLoggingUrl,
    level: NgxLoggerLevel.DEBUG,
    serverLogLevel: NgxLoggerLevel.DEBUG
  }
}

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));