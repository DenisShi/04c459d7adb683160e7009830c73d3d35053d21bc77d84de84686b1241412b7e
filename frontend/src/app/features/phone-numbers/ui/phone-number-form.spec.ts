import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { HarnessLoader } from '@angular/cdk/testing';
import { MatButtonHarness } from '@angular/material/button/testing';
import { MatInputHarness } from '@angular/material/input/testing';
import { MatRadioGroupHarness } from '@angular/material/radio/testing';
import { CreatePhoneNumberRequest } from '../data-access/phone-number';
import { PhoneNumberForm } from './phone-number-form';

describe('PhoneNumberForm', () => {
  let fixture: ComponentFixture<PhoneNumberForm>;
  let loader: HarnessLoader;
  let submitted: CreatePhoneNumberRequest[];

  beforeEach(() => {
    fixture = TestBed.createComponent(PhoneNumberForm);
    loader = TestbedHarnessEnvironment.loader(fixture);
    fixture.detectChanges();
    submitted = [];
    fixture.componentInstance.submitted.subscribe((request) => submitted.push(request));
  });

  function nameInput(): Promise<MatInputHarness> {
    return loader.getHarness(MatInputHarness.with({ selector: '[formControlName="contactName"]' }));
  }

  function numberInput(): Promise<MatInputHarness> {
    return loader.getHarness(MatInputHarness.with({ selector: '[formControlName="number"]' }));
  }

  async function submit(): Promise<void> {
    await (await loader.getHarness(MatButtonHarness.with({ text: 'Add' }))).click();
  }

  function text(): string {
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  it('defaults visibility to Personal', async () => {
    const group = await loader.getHarness(MatRadioGroupHarness);

    expect(await group.getCheckedValue()).toBe('PERSONAL');
  });

  it('blocks submit and shows errors while the form is invalid', async () => {
    await submit();

    expect(submitted).toEqual([]);
    expect(text()).toContain('Contact name is required.');
    expect(text()).toContain('Phone number is required.');
  });

  it('shows the phone number format error for an invalid number', async () => {
    await (await nameInput()).setValue('Alice');
    await (await numberInput()).setValue('12');
    await submit();

    expect(submitted).toEqual([]);
    expect(text()).toContain('Enter a valid phone number');
  });

  it('emits the trimmed values with the chosen visibility', async () => {
    await (await nameInput()).setValue('  Alice Anderson ');
    await (await numberInput()).setValue(' +420 601 234 567 ');
    await (await loader.getHarness(MatRadioGroupHarness)).checkRadioButton({ label: 'Shared' });
    await submit();

    expect(submitted).toEqual([
      { contactName: 'Alice Anderson', number: '+420 601 234 567', visibility: 'SHARED' },
    ]);
  });

  it('maps server errors onto the matching fields', async () => {
    fixture.componentRef.setInput('serverErrors', { number: 'The server rejected this number.' });
    await fixture.whenStable();

    expect(text()).toContain('The server rejected this number.');
  });

  it('rejects a contact name longer than 100 characters and accepts exactly 100', async () => {
    await (await numberInput()).setValue('+420601234567');
    await (await nameInput()).setValue('a'.repeat(101));
    await submit();

    expect(submitted).toEqual([]);
    expect(text()).toContain('Contact name must be at most 100 characters.');

    await (await nameInput()).setValue('a'.repeat(100));
    await submit();

    expect(submitted).toHaveLength(1);
  });

  it('rejects a phone number longer than 32 characters', async () => {
    await (await nameInput()).setValue('Alice');
    await (await numberInput()).setValue(`+${'1'.repeat(32)}`);
    await submit();

    expect(submitted).toEqual([]);
    expect(text()).toContain('Phone number must be at most 32 characters.');
  });

  it('rejects a whitespace-only contact name', async () => {
    await (await nameInput()).setValue('   ');
    await (await numberInput()).setValue('+420601234567');
    await submit();

    expect(submitted).toEqual([]);
    expect(text()).toContain('Contact name is required.');
  });

  it('shows server errors for the contact name and the visibility', async () => {
    fixture.componentRef.setInput('serverErrors', {
      contactName: 'The server rejected this name.',
      visibility: 'The server rejected this visibility.',
    });
    await fixture.whenStable();

    expect(text()).toContain('The server rejected this name.');
    expect(text()).toContain('The server rejected this visibility.');
  });

  it('explains who can see the entry for each visibility', async () => {
    expect(text()).toContain('Only you can see this entry.');

    await (await loader.getHarness(MatRadioGroupHarness)).checkRadioButton({ label: 'Shared' });

    expect(text()).toContain('Everyone who signs in can see this entry.');
  });

  it('does not send owner information', async () => {
    await (await nameInput()).setValue('Alice');
    await (await numberInput()).setValue('+420601234567');
    await submit();

    expect(Object.keys(submitted[0]).sort()).toEqual(['contactName', 'number', 'visibility']);
  });

  it('shows a general error when saving failed', async () => {
    fixture.componentRef.setInput('failed', true);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'could not be saved',
    );
  });

  it('resets to the defaults', async () => {
    await (await nameInput()).setValue('Alice');
    await (await loader.getHarness(MatRadioGroupHarness)).checkRadioButton({ label: 'Shared' });

    fixture.componentInstance.reset();

    expect(await (await nameInput()).getValue()).toBe('');
    expect(await (await loader.getHarness(MatRadioGroupHarness)).getCheckedValue()).toBe(
      'PERSONAL',
    );
  });

  it('clears validation errors after a submitted form is reset', async () => {
    await (await nameInput()).setValue('Alice');
    await (await numberInput()).setValue('+420601234567');
    await submit();

    fixture.componentInstance.reset();
    await fixture.whenStable();

    expect(text()).not.toContain('is required.');
    expect(
      fixture.nativeElement.querySelector('.mat-form-field-invalid, .mat-mdc-form-field-error'),
    ).toBeNull();
  });
});
