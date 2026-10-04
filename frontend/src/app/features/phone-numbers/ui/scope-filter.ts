import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { PhoneNumberScope } from '../data-access/phone-number';

@Component({
  selector: 'app-scope-filter',
  imports: [MatButtonToggleGroup, MatButtonToggle],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-button-toggle-group
      aria-label="Filter by visibility"
      [value]="scope()"
      (change)="scopeChange.emit($event.value)"
    >
      <mat-button-toggle value="all">All</mat-button-toggle>
      <mat-button-toggle value="personal">Personal</mat-button-toggle>
      <mat-button-toggle value="shared">Shared</mat-button-toggle>
    </mat-button-toggle-group>
  `,
})
export class ScopeFilter {
  readonly scope = input.required<PhoneNumberScope>();
  readonly scopeChange = output<PhoneNumberScope>();
}
