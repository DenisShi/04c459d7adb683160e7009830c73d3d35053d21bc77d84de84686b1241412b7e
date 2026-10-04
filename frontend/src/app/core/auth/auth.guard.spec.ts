import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import {
  FakeKeycloak,
  createFakeKeycloak,
  provideFakeKeycloak,
} from '../../../testing/fake-keycloak';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let keycloak: FakeKeycloak;

  function runGuard(url: string): Promise<unknown> {
    const route = {} as ActivatedRouteSnapshot;
    const state = { url } as RouterStateSnapshot;
    return Promise.resolve(TestBed.runInInjectionContext(() => authGuard(route, state)));
  }

  beforeEach(() => {
    keycloak = createFakeKeycloak();
    TestBed.configureTestingModule({ providers: [provideFakeKeycloak(keycloak)] });
  });

  it('allows an authenticated user', async () => {
    keycloak.authenticated = true;

    await expect(runGuard('/phone-numbers')).resolves.toBe(true);
    expect(keycloak.login).not.toHaveBeenCalled();
  });

  it('starts a login with the requested URL and blocks navigation for an anonymous user', async () => {
    await expect(runGuard('/phone-numbers?scope=shared')).resolves.toBe(false);

    expect(keycloak.login).toHaveBeenCalledWith({
      redirectUri: new URL('/phone-numbers?scope=shared', document.baseURI).href,
    });
  });
});
