import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { APP_CONFIG } from './core/tokens/app-config.token';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    {
      provide: APP_CONFIG,
      useValue: {
        apiBaseUrl: 'http://localhost:5098/api'
      }
    }
  ]
};
