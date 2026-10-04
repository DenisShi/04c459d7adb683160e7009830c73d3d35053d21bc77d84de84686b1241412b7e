import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PhoneNumber } from '../data-access/phone-number';
import { PhoneNumberList } from './phone-number-list';

function phoneNumber(overrides: Partial<PhoneNumber>): PhoneNumber {
  return {
    id: '1',
    contactName: 'Alice Anderson',
    number: '+420601234567',
    visibility: 'PERSONAL',
    ownerUsername: 'alice',
    isOwnedByCurrentUser: true,
    createdAt: '2026-10-02T12:34:56.789Z',
    ...overrides,
  };
}

describe('PhoneNumberList', () => {
  let fixture: ComponentFixture<PhoneNumberList>;

  beforeEach(() => {
    fixture = TestBed.createComponent(PhoneNumberList);
    fixture.componentRef.setInput('phoneNumbers', []);
  });

  async function render(): Promise<HTMLElement> {
    fixture.detectChanges();
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('renders a row per entry with a visibility chip and the owner', async () => {
    fixture.componentRef.setInput('phoneNumbers', [
      phoneNumber({ id: '1', contactName: 'Alice Anderson' }),
      phoneNumber({
        id: '2',
        contactName: 'Bob Brown',
        visibility: 'SHARED',
        ownerUsername: 'bob',
        isOwnedByCurrentUser: false,
      }),
    ]);

    const element = await render();
    const rows = element.querySelectorAll('tr[mat-row]');

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Personal');
    expect(rows[0].textContent).toContain('You');
    expect(rows[1].textContent).toContain('Shared');
    expect(rows[1].textContent).toContain('bob');
    expect(rows[1].textContent).not.toContain('You');
  });

  it('shows the empty state for the all scope', async () => {
    const element = await render();

    expect(element.querySelector('[data-testid="empty-state"]')?.textContent).toContain(
      'No phone numbers yet.',
    );
  });

  it('names the scope in the empty state', async () => {
    fixture.componentRef.setInput('scope', 'shared');

    const element = await render();

    expect(element.textContent).toContain('No shared phone numbers yet.');
  });

  it('shows a loading indicator and no empty state while loading', async () => {
    fixture.componentRef.setInput('loading', true);

    const element = await render();

    expect(element.querySelector('mat-progress-bar')).not.toBeNull();
    expect(element.querySelector('[data-testid="empty-state"]')).toBeNull();
  });

  it('shows an error with a retry button that emits retry', async () => {
    fixture.componentRef.setInput('failed', true);
    let retried = 0;
    fixture.componentInstance.retry.subscribe(() => retried++);

    const element = await render();
    element.querySelector<HTMLButtonElement>('button')?.click();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('could not be loaded');
    expect(retried).toBe(1);
  });

  it('offers no edit or delete controls', async () => {
    fixture.componentRef.setInput('phoneNumbers', [phoneNumber({})]);

    const element = await render();

    expect(element.querySelectorAll('button')).toHaveLength(0);
  });
});
