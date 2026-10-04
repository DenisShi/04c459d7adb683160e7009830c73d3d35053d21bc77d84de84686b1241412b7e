import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatProgressBar } from '@angular/material/progress-bar';
import { PhoneNumbersApi } from '../data-access/phone-numbers-api';

@Component({
  selector: 'app-phone-numbers-page',
  imports: [MatProgressBar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="page">
      <h1 class="title">Phone numbers</h1>
      @if (phoneNumbers.isLoading()) {
        <mat-progress-bar mode="indeterminate" aria-label="Loading phone numbers" />
      } @else if (phoneNumbers.error()) {
        <p role="alert">Phone numbers could not be loaded. Try again later.</p>
      } @else {
        <p data-testid="phone-numbers-summary">{{ summary() }}</p>
      }
    </section>
  `,
  styles: `
    .page {
      max-width: 64rem;
      margin: 0 auto;
      padding: 1.5rem;
    }

    .title {
      margin: 0 0 1rem;
      font: var(--mat-sys-headline-small);
    }
  `,
})
export class PhoneNumbersPage {
  private readonly api = inject(PhoneNumbersApi);

  protected readonly phoneNumbers = rxResource({
    stream: () => this.api.list('all'),
  });

  protected readonly summary = computed(() => {
    const count = this.phoneNumbers.hasValue() ? this.phoneNumbers.value().length : 0;
    if (count === 0) {
      return 'No phone numbers yet.';
    }
    return count === 1
      ? '1 phone number is visible to you.'
      : `${count} phone numbers are visible to you.`;
  });
}
