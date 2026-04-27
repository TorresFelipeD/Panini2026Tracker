import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { APP_CONFIG } from './core/tokens/app-config.token';

function resolveApiBaseUrl(): string {
  return window.location.port === '4200'
    ? 'http://localhost:5098/api'
    : '/api';
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    {
      provide: APP_CONFIG,
      useValue: {
        apiBaseUrl: resolveApiBaseUrl()
      }
    }
  ]
};
