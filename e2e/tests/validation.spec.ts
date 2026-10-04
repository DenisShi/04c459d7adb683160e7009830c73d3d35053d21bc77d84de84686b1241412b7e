import { expect, test } from '@playwright/test';
import { PhoneNumbersPage } from '../pages/phone-numbers.page';
import { uniqueContactName } from '../support/unique-name';

test.describe('add form validation', () => {
  let createRequests: string[];

  test.beforeEach(async ({ page }) => {
    createRequests = [];
    page.on('request', (request) => {
      if (request.method() === 'POST' && new URL(request.url()).pathname.startsWith('/api/')) {
        createRequests.push(request.url());
      }
    });
    await new PhoneNumbersPage(page).goto();
  });

  test('empty form shows required errors and sends nothing', async ({ page }) => {
    const { addForm } = new PhoneNumbersPage(page);

    await addForm.submit();

    await expect(addForm.contactNameRequiredError).toBeVisible();
    await expect(addForm.numberRequiredError).toBeVisible();
    expect(createRequests).toEqual([]);
  });

  test('invalid phone number shows an error and the entry is not saved', async ({ page }) => {
    const phoneNumbersPage = new PhoneNumbersPage(page);
    const contactName = uniqueContactName('Invalid');

    await phoneNumbersPage.addForm.add({ contactName, number: '12', visibility: 'Shared' });

    await expect(phoneNumbersPage.addForm.numberInvalidError).toBeVisible();
    await expect(phoneNumbersPage.addForm.contactNameRequiredError).toHaveCount(0);
    expect(createRequests).toEqual([]);

    await page.reload();
    await expect(phoneNumbersPage.heading).toBeVisible();
    await phoneNumbersPage.expectNoEntry(contactName);
  });

  test('blank contact name shows a required error', async ({ page }) => {
    const { addForm } = new PhoneNumbersPage(page);

    await addForm.add({ contactName: '   ', number: '601 234 567', visibility: 'Personal' });

    await expect(addForm.contactNameRequiredError).toBeVisible();
    expect(createRequests).toEqual([]);
  });
});
