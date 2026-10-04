import { expect, test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { uniqueContactName } from '../support/unique-name';

test('alice adds a personal and a shared phone number', async ({ page }) => {
  const phoneNumbersPage = new PhoneNumbersPage(page);
  await phoneNumbersPage.goto();
  await expect(phoneNumbersPage.addForm.visibilityOption('Personal')).toBeChecked();

  const personalName = uniqueContactName('Personal');
  const sharedName = uniqueContactName('Shared');

  await phoneNumbersPage.add({
    contactName: personalName,
    number: '+420 601 234 567',
    visibility: 'Personal',
  });
  await phoneNumbersPage.add({
    contactName: sharedName,
    number: '(02) 1234-5678',
    visibility: 'Shared',
  });

  await page.reload();
  await phoneNumbersPage.expectEntry(personalName, 'Personal');
  await phoneNumbersPage.expectEntry(sharedName, 'Shared');
  await expect(
    phoneNumbersPage.row(personalName).getByRole('cell', { name: '+420601234567', exact: true }),
  ).toBeVisible();
  await expect(
    phoneNumbersPage.row(sharedName).getByRole('cell', { name: '0212345678', exact: true }),
  ).toBeVisible();
});
