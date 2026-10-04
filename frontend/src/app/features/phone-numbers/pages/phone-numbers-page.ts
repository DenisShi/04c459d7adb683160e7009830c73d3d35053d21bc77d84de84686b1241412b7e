import { ChangeDetectionStrategy, Component, inject, viewChild } from '@angular/core';
import { CreatePhoneNumberRequest } from '../data-access/phone-number';
import { PhoneNumbersStore } from '../data-access/phone-numbers-store';
import { PhoneNumberForm } from '../ui/phone-number-form';
import { PhoneNumberList } from '../ui/phone-number-list';
import { ScopeFilter } from '../ui/scope-filter';

@Component({
  selector: 'app-phone-numbers-page',
  imports: [PhoneNumberForm, PhoneNumberList, ScopeFilter],
  providers: [PhoneNumbersStore],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <main class="page">
      <h1 class="title">Phone numbers</h1>
      <div class="layout">
        <section class="panel form-panel" aria-labelledby="add-heading">
          <h2 id="add-heading" class="panel-title">Add a phone number</h2>
          <app-phone-number-form
            [saving]="store.saving()"
            [failed]="store.saveFailed()"
            [serverErrors]="store.fieldErrors()"
            (submitted)="add($event)"
          />
        </section>
        <section class="panel" aria-labelledby="list-heading">
          <div class="list-header">
            <h2 id="list-heading" class="panel-title">Entries</h2>
            <app-scope-filter [scope]="store.scope()" (scopeChange)="store.selectScope($event)" />
          </div>
          <app-phone-number-list
            [phoneNumbers]="store.phoneNumbers()"
            [scope]="store.scope()"
            [loading]="store.loading()"
            [failed]="store.loadFailed()"
            (retry)="store.reload()"
          />
        </section>
      </div>
    </main>
  `,
  styles: `
    .page {
      display: block;
      max-width: 72rem;
      margin: 0 auto;
      padding: 1.5rem 1rem;
    }

    .title {
      margin: 0 0 1rem;
      font: var(--mat-sys-headline-small);
    }

    .layout {
      display: grid;
      gap: 1.5rem;
      align-items: start;
    }

    .panel {
      min-width: 0;
      padding: 1.25rem;
      border-radius: var(--mat-sys-corner-large);
      background: var(--mat-sys-surface-container-low);
    }

    .panel-title {
      margin: 0;
      font: var(--mat-sys-title-medium);
    }

    .form-panel .panel-title {
      margin-bottom: 1rem;
    }

    .list-header {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      margin-bottom: 1rem;
    }

    @media (min-width: 60rem) {
      .layout {
        grid-template-columns: 20rem 1fr;
      }
    }
  `,
})
export class PhoneNumbersPage {
  protected readonly store = inject(PhoneNumbersStore);
  private readonly form = viewChild.required(PhoneNumberForm);

  protected async add(request: CreatePhoneNumberRequest): Promise<void> {
    if (await this.store.create(request)) {
      this.form().reset();
    }
  }
}
