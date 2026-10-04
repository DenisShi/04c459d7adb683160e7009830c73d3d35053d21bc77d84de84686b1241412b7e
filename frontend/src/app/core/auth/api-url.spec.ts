import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  includeBearerTokenInterceptor,
} from 'keycloak-angular';
import { firstValueFrom } from 'rxjs';
import {
  FakeKeycloak,
  createFakeKeycloak,
  provideFakeKeycloak,
} from '../../../testing/fake-keycloak';
import { apiBearerTokenCondition, isApiUrl } from './api-url';

describe('isApiUrl', () => {
  it.each(['/api', '/api/', '/api/phone-numbers', '/api/phone-numbers/0199a1b2?x=1'])(
    'matches %s',
    (url) => {
      expect(isApiUrl(url)).toBe(true);
    },
  );

  it.each([
    '/config.json',
    '/apiary',
    '/silent-check-sso.html',
    'api/phone-numbers',
    'http://localhost:8080/realms/phonebook/protocol/openid-connect/token',
    'http://evil.example/api/phone-numbers',
  ])('does not match %s', (url) => {
    expect(isApiUrl(url)).toBe(false);
  });
});

describe('apiBearerTokenCondition', () => {
  let keycloak: FakeKeycloak;
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    keycloak = createFakeKeycloak({ authenticated: true, token: 'access-token' });
    TestBed.configureTestingModule({
      providers: [
        provideFakeKeycloak(keycloak),
        { provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG, useValue: [apiBearerTokenCondition] },
        provideHttpClient(withInterceptors([includeBearerTokenInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  async function authorizationHeaderFor(url: string): Promise<string | null> {
    const response = firstValueFrom(http.get(url));
    const request = await vi.waitFor(() => httpTesting.expectOne(url));
    const header = request.request.headers.get('Authorization');
    request.flush({});
    await response;
    return header;
  }

  it.each(['/api', '/api/phone-numbers'])('attaches the bearer token to %s', async (url) => {
    await expect(authorizationHeaderFor(url)).resolves.toBe('Bearer access-token');
    expect(keycloak.updateToken).toHaveBeenCalled();
  });

  it.each([
    '/config.json',
    '/apiary',
    'http://localhost:8080/realms/phonebook/protocol/openid-connect/token',
  ])('does not attach the bearer token to %s', async (url) => {
    await expect(authorizationHeaderFor(url)).resolves.toBeNull();
    expect(keycloak.updateToken).not.toHaveBeenCalled();
  });

  it('does not attach a token when the user is not authenticated', async () => {
    keycloak.authenticated = false;

    await expect(authorizationHeaderFor('/api/phone-numbers')).resolves.toBeNull();
  });
});
