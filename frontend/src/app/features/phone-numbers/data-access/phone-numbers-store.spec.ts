import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { PhoneNumber } from './phone-number';
import { PHONE_NUMBERS_URL } from './phone-numbers-api';
import { PhoneNumbersStore, extractFieldErrors } from './phone-numbers-store';

const entry: PhoneNumber = {
  id: '1',
  contactName: 'Alice Anderson',
  number: '+420601234567',
  visibility: 'PERSONAL',
  ownerUsername: 'alice',
  isOwnedByCurrentUser: true,
  createdAt: '2026-10-02T12:34:56.789Z',
};

const serverError = { status: 500, statusText: 'Server Error' };

describe('PhoneNumbersStore', () => {
  let store: PhoneNumbersStore;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PhoneNumbersStore, provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(PhoneNumbersStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  async function flushList(
    scope: string,
    body: object,
    options?: { status: number; statusText: string },
  ): Promise<void> {
    TestBed.tick();
    const request = httpTesting.expectOne(
      (candidate) =>
        candidate.method === 'GET' &&
        candidate.url === PHONE_NUMBERS_URL &&
        candidate.params.get('scope') === scope,
    );
    request.flush(body, options);
    await TestBed.inject(ApplicationRef).whenStable();
  }

  it('loads the all scope first', async () => {
    await flushList('all', [entry]);

    expect(store.phoneNumbers()).toEqual([entry]);
    expect(store.loading()).toBe(false);
  });

  it('reloads the list when the scope changes', async () => {
    await flushList('all', []);

    store.selectScope('shared');
    await flushList('shared', [entry]);

    expect(store.phoneNumbers()).toEqual([entry]);
  });

  it('surfaces a load failure as state', async () => {
    await flushList('all', {}, serverError);

    expect(store.loadFailed()).toBe(true);
    expect(store.phoneNumbers()).toEqual([]);
  });

  it('recovers from a load failure when reloaded', async () => {
    await flushList('all', {}, serverError);

    store.reload();
    await flushList('all', [entry]);

    expect(store.loadFailed()).toBe(false);
    expect(store.phoneNumbers()).toEqual([entry]);
  });

  it('reloads the list after a successful create', async () => {
    await flushList('all', []);

    const created = store.create({
      contactName: 'Alice Anderson',
      number: '+420 601 234 567',
      visibility: 'PERSONAL',
    });
    httpTesting.expectOne({ method: 'POST', url: PHONE_NUMBERS_URL }).flush(entry);

    await expect(created).resolves.toBe(true);
    await flushList('all', [entry]);
    expect(store.phoneNumbers()).toEqual([entry]);
    expect(store.saving()).toBe(false);
  });

  it('maps validation problems onto fields without reloading', async () => {
    await flushList('all', []);

    const created = store.create({ contactName: 'A', number: '12', visibility: 'PERSONAL' });
    httpTesting
      .expectOne({ method: 'POST', url: PHONE_NUMBERS_URL })
      .flush(
        { errors: { number: ['Enter a valid phone number.'] } },
        { status: 400, statusText: 'Bad Request' },
      );

    await expect(created).resolves.toBe(false);
    expect(store.fieldErrors()).toEqual({ number: 'Enter a valid phone number.' });
    expect(store.saveFailed()).toBe(false);
  });

  it('flags a general failure for other errors', async () => {
    await flushList('all', []);

    const created = store.create({ contactName: 'A', number: '112', visibility: 'PERSONAL' });
    httpTesting.expectOne({ method: 'POST', url: PHONE_NUMBERS_URL }).flush({}, serverError);

    await expect(created).resolves.toBe(false);
    expect(store.saveFailed()).toBe(true);
    expect(store.fieldErrors()).toEqual({});
  });
});

describe('extractFieldErrors', () => {
  it('ignores unknown keys and non-validation responses', () => {
    const unknownKey = new HttpErrorResponse({ status: 400, error: { errors: { other: ['x'] } } });
    const wrongStatus = new HttpErrorResponse({
      status: 500,
      error: { errors: { number: ['x'] } },
    });

    expect(extractFieldErrors(unknownKey)).toEqual({});
    expect(extractFieldErrors(wrongStatus)).toEqual({});
    expect(extractFieldErrors(new Error('boom'))).toEqual({});
  });

  it('returns no field errors for a bad request without an errors object', () => {
    const withoutErrors = new HttpErrorResponse({ status: 400, error: { title: 'Bad Request' } });
    const withNullBody = new HttpErrorResponse({ status: 400, error: null });

    expect(extractFieldErrors(withoutErrors)).toEqual({});
    expect(extractFieldErrors(withNullBody)).toEqual({});
  });

  it('keeps only the first message of each known field', () => {
    const response = new HttpErrorResponse({
      status: 400,
      error: { errors: { contactName: ['first', 'second'], number: [], visibility: ['bad'] } },
    });

    expect(extractFieldErrors(response)).toEqual({ contactName: 'first', visibility: 'bad' });
  });
});
