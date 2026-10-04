import { TestBed } from '@angular/core/testing';
import { KeycloakEventType } from 'keycloak-angular';
import {
  FakeKeycloak,
  createFakeKeycloak,
  createKeycloakEvent,
  provideFakeKeycloak,
} from '../../../testing/fake-keycloak';
import { AuthSession } from './auth-session';

describe('AuthSession', () => {
  let keycloak: FakeKeycloak;
  let event: ReturnType<typeof createKeycloakEvent>;
  let session: AuthSession;

  beforeEach(() => {
    keycloak = createFakeKeycloak();
    event = createKeycloakEvent();
    TestBed.configureTestingModule({ providers: [provideFakeKeycloak(keycloak, event)] });
    session = TestBed.inject(AuthSession);
  });

  it('reports an anonymous user as not authenticated with an empty username', () => {
    expect(session.authenticated()).toBe(false);
    expect(session.username()).toBe('');
  });

  it('uses preferred_username as the username', () => {
    keycloak.authenticated = true;
    keycloak.tokenParsed = { sub: 'alice-id', preferred_username: 'alice' };
    event.set({ type: KeycloakEventType.AuthSuccess });

    expect(session.authenticated()).toBe(true);
    expect(session.username()).toBe('alice');
  });

  it('falls back to sub when preferred_username is missing', () => {
    keycloak.authenticated = true;
    keycloak.tokenParsed = { sub: 'alice-id' };
    event.set({ type: KeycloakEventType.AuthSuccess });

    expect(session.username()).toBe('alice-id');
  });

  it('updates the username after a token refresh', () => {
    keycloak.authenticated = true;
    keycloak.tokenParsed = { sub: 'alice-id', preferred_username: 'alice' };
    event.set({ type: KeycloakEventType.AuthSuccess });
    expect(session.username()).toBe('alice');

    keycloak.tokenParsed = { sub: 'alice-id', preferred_username: 'alice.anderson' };
    event.set({ type: KeycloakEventType.AuthRefreshSuccess });

    expect(session.username()).toBe('alice.anderson');
  });

  it('signs out and returns to the application origin', async () => {
    await session.signOut();

    expect(keycloak.logout).toHaveBeenCalledWith({ redirectUri: document.location.origin });
  });
});
