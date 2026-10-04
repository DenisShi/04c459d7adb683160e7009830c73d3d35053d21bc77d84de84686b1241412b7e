import { expect, test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';

test('a loading indicator is shown while the session is restored on reload', async ({ page }) => {
  let releaseTokenRequest = (): void => undefined;
  const tokenRequestReleased = new Promise<void>((resolve) => {
    releaseTokenRequest = resolve;
  });
  await page.route('**/protocol/openid-connect/token', async (route) => {
    await tokenRequestReleased;
    await route.continue();
  });
  const phoneNumbersPage = new PhoneNumbersPage(page);

  await page.goto('/phone-numbers', { waitUntil: 'commit' });

  await expect(phoneNumbersPage.loadingIndicator).toBeVisible();
  await expect(phoneNumbersPage.heading).toHaveCount(0);

  releaseTokenRequest();

  await expect(phoneNumbersPage.heading).toBeVisible();
  await expect(phoneNumbersPage.loadingIndicator).toHaveCount(0);
});
