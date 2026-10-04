import { EnvironmentProviders } from '@angular/core';
import {
  AutoRefreshTokenService,
  INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  UserActivityService,
  provideKeycloak,
  withAutoRefreshToken,
} from 'keycloak-angular';
import { KeycloakRuntimeConfig } from '../config/runtime-config';
import { apiBearerTokenCondition } from './api-url';

export const SESSION_IDLE_TIMEOUT_MS = 30 * 60 * 1000;

export function provideAuth(config: KeycloakRuntimeConfig, origin: string): EnvironmentProviders {
  return provideKeycloak({
    config: {
      url: config.url,
      realm: config.realm,
      clientId: config.clientId,
    },
    initOptions: {
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri: `${origin}/silent-check-sso.html`,
      pkceMethod: 'S256',
      checkLoginIframe: false,
    },
    features: [
      withAutoRefreshToken({
        onInactivityTimeout: 'logout',
        sessionTimeout: SESSION_IDLE_TIMEOUT_MS,
        logoutOptions: { redirectUri: origin },
      }),
    ],
    providers: [
      AutoRefreshTokenService,
      UserActivityService,
      {
        provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
        useValue: [apiBearerTokenCondition],
      },
    ],
  });
}
