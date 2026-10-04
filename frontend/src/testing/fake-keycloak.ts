import { Provider, WritableSignal, signal } from '@angular/core';
import { KEYCLOAK_EVENT_SIGNAL, KeycloakEvent, KeycloakEventType } from 'keycloak-angular';
import Keycloak, { KeycloakTokenParsed } from 'keycloak-js';

export interface FakeKeycloak {
  authenticated: boolean;
  token: string | undefined;
  tokenParsed: KeycloakTokenParsed | undefined;
  login: ReturnType<typeof vi.fn<Keycloak['login']>>;
  logout: ReturnType<typeof vi.fn<Keycloak['logout']>>;
  updateToken: ReturnType<typeof vi.fn<Keycloak['updateToken']>>;
}

export function createFakeKeycloak(overrides: Partial<FakeKeycloak> = {}): FakeKeycloak {
  return {
    authenticated: false,
    token: undefined,
    tokenParsed: undefined,
    login: vi.fn<Keycloak['login']>().mockResolvedValue(undefined),
    logout: vi.fn<Keycloak['logout']>().mockResolvedValue(undefined),
    updateToken: vi.fn<Keycloak['updateToken']>().mockResolvedValue(true),
    ...overrides,
  };
}

export function createKeycloakEvent(
  type: KeycloakEventType = KeycloakEventType.Ready,
): WritableSignal<KeycloakEvent> {
  return signal<KeycloakEvent>({ type });
}

export function provideFakeKeycloak(
  keycloak: FakeKeycloak,
  event: WritableSignal<KeycloakEvent> = createKeycloakEvent(),
): Provider[] {
  return [
    { provide: Keycloak, useValue: keycloak },
    { provide: KEYCLOAK_EVENT_SIGNAL, useValue: event },
  ];
}
