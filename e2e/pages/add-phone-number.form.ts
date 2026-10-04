import type { Locator, Page } from '@playwright/test';

export type Visibility = 'Personal' | 'Shared';

export interface NewPhoneNumber {
  readonly contactName: string;
  readonly number: string;
  readonly visibility: Visibility;
}

export class AddPhoneNumberForm {
  readonly root: Locator;
  readonly contactNameInput: Locator;
  readonly numberInput: Locator;
  readonly submitButton: Locator;
  readonly contactNameRequiredError: Locator;
  readonly numberRequiredError: Locator;
  readonly numberInvalidError: Locator;

  constructor(page: Page) {
    this.root = page.getByRole('form', { name: 'Add phone number' });
    this.contactNameInput = this.root.getByLabel('Contact name');
    this.numberInput = this.root.getByLabel('Phone number');
    this.submitButton = this.root.getByRole('button', { name: 'Add', exact: true });
    this.contactNameRequiredError = this.root.getByText('Contact name is required.');
    this.numberRequiredError = this.root.getByText('Phone number is required.');
    this.numberInvalidError = this.root.getByText('Enter a valid phone number', { exact: false });
  }

  visibilityOption(visibility: Visibility): Locator {
    return this.root
      .getByRole('radiogroup', { name: 'Visibility' })
      .getByRole('radio', { name: visibility, exact: true });
  }

  async submit(): Promise<void> {
    await this.submitButton.click();
  }

  async add(entry: NewPhoneNumber): Promise<void> {
    await this.contactNameInput.fill(entry.contactName);
    await this.numberInput.fill(entry.number);
    await this.visibilityOption(entry.visibility).check();
    await this.submit();
  }
}
