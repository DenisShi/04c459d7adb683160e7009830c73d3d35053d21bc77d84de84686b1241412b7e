import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatToolbar } from '@angular/material/toolbar';
import { AuthSession } from '../auth/auth-session';

@Component({
  selector: 'app-header',
  imports: [MatToolbar, MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-toolbar class="header">
      <span class="title">Phone Book</span>
      <span class="spacer"></span>
      <span class="username" data-testid="current-username">{{ session.username() }}</span>
      <button matButton="outlined" type="button" (click)="signOut()">Sign out</button>
    </mat-toolbar>
  `,
  styles: `
    .header {
      gap: 1rem;
      background: var(--mat-sys-surface-container);
    }

    .spacer {
      flex: 1;
    }

    .username {
      font: var(--mat-sys-title-small);
    }
  `,
})
export class Header {
  protected readonly session = inject(AuthSession);

  protected signOut(): void {
    void this.session.signOut();
  }
}
