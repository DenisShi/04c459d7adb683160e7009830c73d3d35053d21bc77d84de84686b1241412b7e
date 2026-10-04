import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthSession } from './core/auth/auth-session';
import { Header } from './core/layout/header';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (session.authenticated()) {
      <app-header />
    }
    <main>
      <router-outlet />
    </main>
  `,
})
export class App {
  protected readonly session = inject(AuthSession);
}
