import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { includeBearerTokenInterceptor } from 'keycloak-angular';
import { routes } from './app.routes';
import { provideAuth } from './core/auth/provide-auth';
import { sessionExpiredInterceptor } from './core/auth/session-expired.interceptor';
import { RuntimeConfig } from './core/config/runtime-config';

export function createAppConfig(config: RuntimeConfig, origin: string): ApplicationConfig {
  return {
    providers: [
      provideBrowserGlobalErrorListeners(),
      provideAuth(config.keycloak, origin),
      provideHttpClient(
        withFetch(),
        withInterceptors([sessionExpiredInterceptor, includeBearerTokenInterceptor]),
      ),
      provideRouter(routes),
    ],
  };
}
