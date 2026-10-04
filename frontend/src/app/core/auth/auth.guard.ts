import { DOCUMENT, inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { createAuthGuard } from 'keycloak-angular';

export const authGuard = createAuthGuard<CanActivateFn>(
  async (_route, state, { authenticated, keycloak }) => {
    if (authenticated) {
      return true;
    }

    const redirectUri = new URL(state.url, inject(DOCUMENT).baseURI).href;
    await keycloak.login({ redirectUri });
    return false;
  },
);
