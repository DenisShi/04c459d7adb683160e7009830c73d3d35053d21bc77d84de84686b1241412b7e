import { DOCUMENT, Injectable, computed, inject } from '@angular/core';
import { KEYCLOAK_EVENT_SIGNAL } from 'keycloak-angular';
import Keycloak from 'keycloak-js';

@Injectable({ providedIn: 'root' })
export class AuthSession {
  private readonly keycloak = inject(Keycloak);
  private readonly keycloakEvent = inject(KEYCLOAK_EVENT_SIGNAL);
  private readonly document = inject(DOCUMENT);

  readonly authenticated = computed(() => {
    this.keycloakEvent();
    return this.keycloak.authenticated === true;
  });

  readonly username = computed(() => {
    this.keycloakEvent();
    const token = this.keycloak.tokenParsed;
    const preferredUsername: unknown = token?.['preferred_username'];
    if (typeof preferredUsername === 'string' && preferredUsername !== '') {
      return preferredUsername;
    }
    return token?.sub ?? '';
  });

  signOut(): Promise<void> {
    return this.keycloak.logout({ redirectUri: this.document.location.origin });
  }
}
