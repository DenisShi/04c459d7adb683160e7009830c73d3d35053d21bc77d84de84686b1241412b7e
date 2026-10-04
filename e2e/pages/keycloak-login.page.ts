import { expect, type Locator, type Page } from '@playwright/test';
import type { TestUser } from '../support/users';

export class LoginPage {
  readonly heading: Locator;
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly signInButton: Locator;

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Sign in to your account' });
    this.usernameInput = page.getByLabel('Username or email');
    this.passwordInput = page.getByLabel('Password', { exact: true });
    this.signInButton = page.getByRole('button', { name: 'Sign In' });
  }

  async expectVisible(): Promise<void> {
    await expect(this.page).toHaveURL(/\/realms\/phonebook\/protocol\/openid-connect\/auth/);
    await expect(this.heading).toBeVisible();
  }

  async signIn(user: TestUser): Promise<void> {
    await this.expectVisible();
    await this.usernameInput.fill(user.username);
    await this.passwordInput.fill(user.password);
    await this.signInButton.click();
  }
}
