import {
  HttpClient,
  HttpErrorResponse,
  HttpStatusCode,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import {
  FakeKeycloak,
  createFakeKeycloak,
  provideFakeKeycloak,
} from '../../../testing/fake-keycloak';
import { sessionExpiredInterceptor } from './session-expired.interceptor';

describe('sessionExpiredInterceptor', () => {
  let keycloak: FakeKeycloak;
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    keycloak = createFakeKeycloak({ authenticated: true });
    TestBed.configureTestingModule({
      providers: [
        provideFakeKeycloak(keycloak),
        provideHttpClient(withInterceptors([sessionExpiredInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  async function failRequest(url: string, status: HttpStatusCode): Promise<HttpErrorResponse> {
    const response = firstValueFrom(http.get(url));
    httpTesting.expectOne(url).flush(null, { status, statusText: 'Error' });
    return response.then(
      () => {
        throw new Error('Expected the request to fail');
      },
      (error: HttpErrorResponse) => error,
    );
  }

  it('starts a login with the current URL when the API answers 401', async () => {
    const error = await failRequest('/api/phone-numbers', HttpStatusCode.Unauthorized);

    expect(error.status).toBe(HttpStatusCode.Unauthorized);
    expect(keycloak.login).toHaveBeenCalledWith({ redirectUri: document.location.href });
  });

  it('ignores 401 responses from non-API URLs', async () => {
    await failRequest('/config.json', HttpStatusCode.Unauthorized);

    expect(keycloak.login).not.toHaveBeenCalled();
  });

  it('ignores other API errors', async () => {
    const error = await failRequest('/api/phone-numbers', HttpStatusCode.InternalServerError);

    expect(error.status).toBe(HttpStatusCode.InternalServerError);
    expect(keycloak.login).not.toHaveBeenCalled();
  });
});
