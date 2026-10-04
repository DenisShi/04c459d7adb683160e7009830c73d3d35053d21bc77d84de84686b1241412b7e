import { expect, test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { uniqueContactName } from '../support/unique-name';

const mutationAffordance = /edit|delete|remove|update|modify|rename/i;

test('the page offers no edit or delete controls', async ({ page }) => {
  const phoneNumbersPage = new PhoneNumbersPage(page);
  const contactName = uniqueContactName('ReadOnly');
  await phoneNumbersPage.goto();
  await phoneNumbersPage.add({ contactName, number: '+420 601 234 569', visibility: 'Shared' });

  for (const role of ['button', 'link', 'menuitem', 'checkbox'] as const) {
    await expect(page.getByRole(role, { name: mutationAffordance })).toHaveCount(0);
  }
  await expect(phoneNumbersPage.row(contactName).getByRole('button')).toHaveCount(0);
  await expect(phoneNumbersPage.row(contactName).getByRole('link')).toHaveCount(0);
  await expect(page.getByRole('columnheader', { name: mutationAffordance })).toHaveCount(0);
});
