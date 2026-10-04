import { expect, test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { uniqueContactName } from '../support/unique-name';

test('filter shows only the entries of the selected scope', async ({ page }) => {
  const personalName = uniqueContactName('Personal');
  const sharedName = uniqueContactName('Shared');

  const phoneNumbersPage = new PhoneNumbersPage(page);
  await phoneNumbersPage.goto();
  await expect(phoneNumbersPage.scopeFilter('All')).toBeChecked();
  await phoneNumbersPage.add({
    contactName: personalName,
    number: '+420 601 234 571',
    visibility: 'Personal',
  });
  await phoneNumbersPage.add({
    contactName: sharedName,
    number: '+420 601 234 572',
    visibility: 'Shared',
  });

  await phoneNumbersPage.filterBy('Personal');
  await phoneNumbersPage.expectEntry(personalName, 'Personal');
  await phoneNumbersPage.expectNoEntry(sharedName);

  await phoneNumbersPage.filterBy('Shared');
  await phoneNumbersPage.expectEntry(sharedName, 'Shared');
  await phoneNumbersPage.expectNoEntry(personalName);

  await phoneNumbersPage.filterBy('All');
  await phoneNumbersPage.expectEntry(personalName, 'Personal');
  await phoneNumbersPage.expectEntry(sharedName, 'Shared');
});
