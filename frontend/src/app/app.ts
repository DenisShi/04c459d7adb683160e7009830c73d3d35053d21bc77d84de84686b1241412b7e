import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, NavigationError, Router, RouterOutlet } from '@angular/router';
import { filter, map, take } from 'rxjs';
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
      @if (!initialNavigationSettled()) {
        <div class="app-loader" role="status" aria-label="Loading Phone Book">
          <span class="app-loader-spinner"></span>
        </div>
      }
      <router-outlet />
    </main>
  `,
})
export class App {
  protected readonly session = inject(AuthSession);
  private readonly router = inject(Router);

  protected readonly initialNavigationSettled = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd || event instanceof NavigationError),
      take(1),
      map(() => true),
    ),
    { initialValue: this.router.navigated },
  );
}
