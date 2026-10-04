import { test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { uniqueContactName } from '../support/unique-name';
import { bob } from '../support/users';

test('bob sees alice shared number and does not see alice personal number', async ({
  page,
  browser,
}) => {
  const personalName = uniqueContactName('Personal');
  const sharedName = uniqueContactName('Shared');

  const alicePage = new PhoneNumbersPage(page);
  await alicePage.goto();
  await alicePage.add({ contactName: personalName, number: '601 234 567', visibility: 'Personal' });
  await alicePage.add({ contactName: sharedName, number: '601 234 568', visibility: 'Shared' });

  const bobContext = await browser.newContext({ storageState: bob.storageStatePath });
  try {
    const bobPage = new PhoneNumbersPage(await bobContext.newPage());
    await bobPage.goto();
    await bobPage.header.expectSignedInAs(bob.username);

    await bobPage.expectEntry(sharedName, 'Shared');
    await bobPage.expectNoEntry(personalName);
  } finally {
    await bobContext.close();
  }
});
