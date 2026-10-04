import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PhoneNumber } from '../data-access/phone-number';
import { PHONE_NUMBERS_URL } from '../data-access/phone-numbers-api';
import { PhoneNumbersPage } from './phone-numbers-page';

function phoneNumber(id: string): PhoneNumber {
  return {
    id,
    contactName: `Contact ${id}`,
    number: '+420601234567',
    visibility: 'SHARED',
    ownerUsername: 'alice',
    isOwnedByCurrentUser: false,
    createdAt: '2026-10-02T12:34:56.789Z',
  };
}

describe('PhoneNumbersPage', () => {
  let fixture: ComponentFixture<PhoneNumbersPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [PhoneNumbersPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(PhoneNumbersPage);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  async function respond(body: object, status = 200): Promise<void> {
    const request = httpTesting.expectOne(
      (candidate) => candidate.url === PHONE_NUMBERS_URL && candidate.params.get('scope') === 'all',
    );
    request.flush(body, { status, statusText: status === 200 ? 'OK' : 'Error' });
    await fixture.whenStable();
  }

  it('shows a loading indicator while the list is loading', () => {
    expect(element().querySelector('mat-progress-bar')).not.toBeNull();
    httpTesting.expectOne(() => true).flush([]);
  });

  it('shows the empty state when no phone numbers are visible', async () => {
    await respond([]);

    expect(element().querySelector('mat-progress-bar')).toBeNull();
    expect(element().textContent).toContain('No phone numbers yet.');
  });

  it('summarizes a single visible phone number', async () => {
    await respond([phoneNumber('1')]);

    expect(element().textContent).toContain('1 phone number is visible to you.');
  });

  it('summarizes several visible phone numbers', async () => {
    await respond([phoneNumber('1'), phoneNumber('2'), phoneNumber('3')]);

    expect(element().textContent).toContain('3 phone numbers are visible to you.');
  });

  it('shows an error when the list cannot be loaded', async () => {
    await respond({ title: 'Server error' }, 500);

    expect(element().querySelector('[role="alert"]')?.textContent).toContain(
      'Phone numbers could not be loaded.',
    );
  });
});
