import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { CreatePhoneNumberRequest, PhoneNumber, PhoneNumberScope } from './phone-number';
import { PHONE_NUMBERS_URL, PhoneNumbersApi } from './phone-numbers-api';

describe('PhoneNumbersApi', () => {
  let api: PhoneNumbersApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(PhoneNumbersApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it.each<PhoneNumberScope>(['all', 'personal', 'shared'])(
    'lists phone numbers with scope %s',
    async (scope) => {
      const phoneNumbers: PhoneNumber[] = [
        {
          id: '0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b',
          contactName: 'Alice Anderson',
          number: '+420601234567',
          visibility: 'PERSONAL',
          ownerUsername: 'alice',
          isOwnedByCurrentUser: true,
          createdAt: '2026-10-02T12:34:56.789Z',
        },
      ];

      const response = firstValueFrom(api.list(scope));
      const request = httpTesting.expectOne(
        (candidate) =>
          candidate.url === PHONE_NUMBERS_URL && candidate.params.get('scope') === scope,
      );
      request.flush(phoneNumbers);

      expect(request.request.method).toBe('GET');
      await expect(response).resolves.toEqual(phoneNumbers);
    },
  );

  it('creates a phone number posting only the contract fields', async () => {
    const created: PhoneNumber = {
      id: '0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b',
      contactName: 'Alice Anderson',
      number: '+420601234567',
      visibility: 'SHARED',
      ownerUsername: 'alice',
      isOwnedByCurrentUser: true,
      createdAt: '2026-10-02T12:34:56.789Z',
    };

    const response = firstValueFrom(
      api.create({
        contactName: 'Alice Anderson',
        number: '+420 601 234 567',
        visibility: 'SHARED',
        ownerUsername: 'mallory',
      } as CreatePhoneNumberRequest),
    );
    const request = httpTesting.expectOne({ method: 'POST', url: PHONE_NUMBERS_URL });
    request.flush(created);

    expect(request.request.body).toEqual({
      contactName: 'Alice Anderson',
      number: '+420 601 234 567',
      visibility: 'SHARED',
    });
    await expect(response).resolves.toEqual(created);
  });
});
