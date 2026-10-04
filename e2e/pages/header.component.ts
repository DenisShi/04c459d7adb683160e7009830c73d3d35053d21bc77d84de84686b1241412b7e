import { expect, type Locator, type Page } from '@playwright/test';

export class HeaderComponent {
  readonly username: Locator;
  readonly signOutButton: Locator;

  constructor(page: Page) {
    this.username = page.getByTestId('current-username');
    this.signOutButton = page.getByRole('button', { name: 'Sign out' });
  }

  async expectSignedInAs(username: string): Promise<void> {
    await expect(this.username).toHaveText(username);
  }

  async signOut(): Promise<void> {
    await this.signOutButton.click();
  }
}
