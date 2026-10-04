import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatChip } from '@angular/material/chips';
import { MatProgressBar } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { PhoneNumber, PhoneNumberScope } from '../data-access/phone-number';

@Component({
  selector: 'app-phone-number-list',
  imports: [DatePipe, MatTableModule, MatChip, MatProgressBar, MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <mat-progress-bar mode="indeterminate" aria-label="Loading phone numbers" />
    }
    @if (failed()) {
      <div class="state" role="alert">
        <p>Phone numbers could not be loaded.</p>
        <button matButton="outlined" type="button" (click)="retry.emit()">Try again</button>
      </div>
    } @else if (phoneNumbers().length > 0) {
      <div class="table-wrapper">
        <table mat-table [dataSource]="phoneNumbers()" [trackBy]="trackById">
          <ng-container matColumnDef="contactName">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let entry">{{ entry.contactName }}</td>
          </ng-container>
          <ng-container matColumnDef="number">
            <th mat-header-cell *matHeaderCellDef>Number</th>
            <td mat-cell *matCellDef="let entry">{{ entry.number }}</td>
          </ng-container>
          <ng-container matColumnDef="visibility">
            <th mat-header-cell *matHeaderCellDef>Visibility</th>
            <td mat-cell *matCellDef="let entry">
              <mat-chip [class]="entry.visibility === 'SHARED' ? 'shared' : 'personal'">
                {{ entry.visibility === 'SHARED' ? 'Shared' : 'Personal' }}
              </mat-chip>
            </td>
          </ng-container>
          <ng-container matColumnDef="owner">
            <th mat-header-cell *matHeaderCellDef>Owner</th>
            <td mat-cell *matCellDef="let entry">
              {{ entry.ownerUsername }}
              @if (entry.isOwnedByCurrentUser) {
                <span class="you">You</span>
              }
            </td>
          </ng-container>
          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>Created</th>
            <td mat-cell *matCellDef="let entry">{{ entry.createdAt | date: 'medium' }}</td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns"></tr>
        </table>
      </div>
    } @else if (!loading()) {
      <p class="state" data-testid="empty-state">{{ emptyMessage() }}</p>
    }
  `,
  styles: `
    :host {
      display: block;
    }

    .table-wrapper {
      overflow-x: auto;
    }

    table {
      width: 100%;
      background: transparent;
    }

    .state {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.75rem;
      margin: 0;
      padding: 2.5rem 1rem;
      color: var(--mat-sys-on-surface-variant);
      text-align: center;
    }

    .shared {
      --mat-chip-elevated-container-color: var(--mat-sys-tertiary-container);
      --mat-chip-label-text-color: var(--mat-sys-on-tertiary-container);
    }

    .personal {
      --mat-chip-elevated-container-color: var(--mat-sys-secondary-container);
      --mat-chip-label-text-color: var(--mat-sys-on-secondary-container);
    }

    .you {
      margin-left: 0.25rem;
      padding: 0.125rem 0.5rem;
      border-radius: 999px;
      background: var(--mat-sys-primary-container);
      color: var(--mat-sys-on-primary-container);
      font: var(--mat-sys-label-small);
    }
  `,
})
export class PhoneNumberList {
  readonly phoneNumbers = input.required<readonly PhoneNumber[]>();
  readonly scope = input<PhoneNumberScope>('all');
  readonly loading = input(false);
  readonly failed = input(false);
  readonly retry = output<void>();

  protected readonly columns = ['contactName', 'number', 'visibility', 'owner', 'createdAt'];

  protected readonly emptyMessage = computed(() =>
    this.scope() === 'all'
      ? 'No phone numbers yet. Add the first one using the form.'
      : `No ${this.scope()} phone numbers yet.`,
  );

  protected trackById(_index: number, entry: PhoneNumber): string {
    return entry.id;
  }
}
