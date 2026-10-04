import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'phone-numbers' },
  {
    path: 'phone-numbers',
    title: 'Phone numbers - Phone Book',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/phone-numbers/pages/phone-numbers-page').then((m) => m.PhoneNumbersPage),
  },
  { path: '**', redirectTo: 'phone-numbers' },
];
