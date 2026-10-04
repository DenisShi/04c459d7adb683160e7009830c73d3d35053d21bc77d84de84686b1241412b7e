import { expect, test } from '@playwright/test';
import { LoginPage } from '../pages/keycloak-login.page';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { alice } from '../support/users';

test.use({ storageState: { cookies: [], origins: [] } });

test('anonymous visitor is redirected to the Keycloak login page', async ({ page }) => {
  await page.goto('/');

  await new LoginPage(page).expectVisible();
});

test('signed in user sees the username in the header', async ({ page }) => {
  await page.goto('/');
  await new LoginPage(page).signIn(alice);

  const phoneNumbersPage = new PhoneNumbersPage(page);
  await expect(page).toHaveURL(/\/phone-numbers$/);
  await expect(phoneNumbersPage.heading).toBeVisible();
  await phoneNumbersPage.header.expectSignedInAs(alice.username);
});

test('signed out user must sign in again on the next visit', async ({ page }) => {
  await page.goto('/');
  const loginPage = new LoginPage(page);
  await loginPage.signIn(alice);
  const phoneNumbersPage = new PhoneNumbersPage(page);
  await phoneNumbersPage.header.expectSignedInAs(alice.username);

  await phoneNumbersPage.header.signOut();
  await loginPage.expectVisible();

  await page.goto('/phone-numbers');
  await loginPage.expectVisible();
  await expect(phoneNumbersPage.header.username).toHaveCount(0);
});
