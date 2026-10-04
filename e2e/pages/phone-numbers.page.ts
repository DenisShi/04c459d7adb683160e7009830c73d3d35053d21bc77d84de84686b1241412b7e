import { expect, type Locator, type Page } from '@playwright/test';
import { AddPhoneNumberForm, type NewPhoneNumber, type Visibility } from './add-phone-number.form';
import { HeaderComponent } from './header.component';

export type Scope = 'All' | Visibility;

export class PhoneNumbersPage {
  readonly heading: Locator;
  readonly header: HeaderComponent;
  readonly addForm: AddPhoneNumberForm;

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Phone numbers', level: 1 });
    this.header = new HeaderComponent(page);
    this.addForm = new AddPhoneNumberForm(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/phone-numbers');
    await expect(this.heading).toBeVisible();
  }

  scopeFilter(scope: Scope): Locator {
    return this.page
      .getByRole('radiogroup', { name: 'Filter by visibility' })
      .getByRole('radio', { name: scope, exact: true });
  }

  async filterBy(scope: Scope): Promise<void> {
    await this.scopeFilter(scope).click();
    await expect(this.scopeFilter(scope)).toBeChecked();
  }

  row(contactName: string): Locator {
    return this.page
      .getByRole('row')
      .filter({ has: this.page.getByRole('cell', { name: contactName, exact: true }) });
  }

  async add(entry: NewPhoneNumber): Promise<void> {
    await this.addForm.add(entry);
    await this.expectEntry(entry.contactName, entry.visibility);
  }

  async expectEntry(contactName: string, visibility: Visibility): Promise<void> {
    const row = this.row(contactName);
    await expect(row).toBeVisible();
    await expect(row.getByRole('cell', { name: visibility, exact: true })).toBeVisible();
  }

  async expectNoEntry(contactName: string): Promise<void> {
    await expect(this.row(contactName)).toHaveCount(0);
  }
}
