import { test as setup } from '@playwright/test';
import { LoginPage } from '../pages/keycloak-login.page';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { signedInUsers } from '../support/users';

for (const user of signedInUsers) {
  setup(`sign in as ${user.username}`, async ({ page }) => {
    await page.goto('/');
    await new LoginPage(page).signIn(user);

    const phoneNumbersPage = new PhoneNumbersPage(page);
    await phoneNumbersPage.header.expectSignedInAs(user.username);

    await page.context().storageState({ path: user.storageStatePath });
  });
}
