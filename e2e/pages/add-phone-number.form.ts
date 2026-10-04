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

  constructor(page: Page) {
    this.root = page.getByRole('form', { name: 'Add phone number' });
    this.contactNameInput = this.root.getByLabel('Contact name');
    this.numberInput = this.root.getByLabel('Phone number');
    this.submitButton = this.root.getByRole('button', { name: 'Add', exact: true });
  }

  visibilityOption(visibility: Visibility): Locator {
    return this.root
      .getByRole('radiogroup', { name: 'Visibility' })
      .getByRole('radio', { name: visibility, exact: true });
  }

  async add(entry: NewPhoneNumber): Promise<void> {
    await this.contactNameInput.fill(entry.contactName);
    await this.numberInput.fill(entry.number);
    await this.visibilityOption(entry.visibility).check();
    await this.submitButton.click();
  }
}
