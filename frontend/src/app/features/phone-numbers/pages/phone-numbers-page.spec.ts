import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatInputHarness } from '@angular/material/input/testing';
import { PhoneNumber } from '../data-access/phone-number';
import { PHONE_NUMBERS_URL } from '../data-access/phone-numbers-api';
import { PhoneNumbersPage } from './phone-numbers-page';

const entry: PhoneNumber = {
  id: '1',
  contactName: 'Alice Anderson',
  number: '+420601234567',
  visibility: 'PERSONAL',
  ownerUsername: 'alice',
  isOwnedByCurrentUser: true,
  createdAt: '2026-10-02T12:34:56.789Z',
};

describe('PhoneNumbersPage', () => {
  let fixture: ComponentFixture<PhoneNumbersPage>;
  let loader: HarnessLoader;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(PhoneNumbersPage);
    loader = TestbedHarnessEnvironment.loader(fixture);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function expectList(scope: string) {
    TestBed.tick();
    return httpTesting.expectOne(
      (request) =>
        request.method === 'GET' &&
        request.url === PHONE_NUMBERS_URL &&
        request.params.get('scope') === scope,
    );
  }

  function clickButton(text: string): void {
    const buttons = Array.from(element().querySelectorAll<HTMLButtonElement>('button'));
    buttons.find((button) => button.textContent?.trim() === text)?.click();
  }

  it('shows a loading indicator while the list is loading', () => {
    TestBed.tick();
    fixture.detectChanges();

    expect(element().querySelector('mat-progress-bar')).not.toBeNull();
    expectList('all').flush([]);
  });

  it('shows the empty state when no phone numbers are visible', async () => {
    expectList('all').flush([]);
    await fixture.whenStable();

    expect(element().querySelector('mat-progress-bar')).toBeNull();
    expect(element().textContent).toContain('No phone numbers yet.');
  });

  it('shows an error when the list cannot be loaded', async () => {
    expectList('all').flush({}, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(element().querySelector('[role="alert"]')?.textContent).toContain(
      'Phone numbers could not be loaded.',
    );
  });

  it('requests the selected scope when the filter changes', async () => {
    expectList('all').flush([]);
    await fixture.whenStable();

    clickButton('Shared');

    expectList('shared').flush([]);
    await fixture.whenStable();
  });

  it('adds an entry, reloads the list and clears the form', async () => {
    expectList('all').flush([]);
    await fixture.whenStable();
    const name = await loader.getHarness(
      MatInputHarness.with({ selector: '[formControlName="contactName"]' }),
    );
    const number = await loader.getHarness(
      MatInputHarness.with({ selector: '[formControlName="number"]' }),
    );
    await name.setValue('Alice Anderson');
    await number.setValue('+420 601 234 567');

    clickButton('Add');

    const post = httpTesting.expectOne({ method: 'POST', url: PHONE_NUMBERS_URL });
    expect(post.request.body).toEqual({
      contactName: 'Alice Anderson',
      number: '+420 601 234 567',
      visibility: 'PERSONAL',
    });
    post.flush(entry);
    await new Promise((resolve) => setTimeout(resolve));
    expectList('all').flush([entry]);
    await fixture.whenStable();

    expect(element().querySelectorAll('tr[mat-row]')).toHaveLength(1);
    expect(await name.getValue()).toBe('');
  });
});
