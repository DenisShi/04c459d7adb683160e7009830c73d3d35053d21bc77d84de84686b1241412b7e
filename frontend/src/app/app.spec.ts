import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, Routes, provideRouter } from '@angular/router';
import { KeycloakEventType } from 'keycloak-angular';
import {
  createFakeKeycloak,
  createKeycloakEvent,
  provideFakeKeycloak,
} from '../testing/fake-keycloak';
import { App } from './app';

@Component({ template: '<p>Routed content</p>' })
class RoutedContent {}

const routes: Routes = [
  { path: '', component: RoutedContent },
  { path: 'denied', canActivate: [() => false], component: RoutedContent },
  {
    path: 'broken',
    loadComponent: () => Promise.reject(new Error('Chunk failed to load.')),
  },
];

describe('App', () => {
  function create(authenticated: boolean): ComponentFixture<App> {
    const keycloak = createFakeKeycloak({
      authenticated,
      tokenParsed: authenticated ? { sub: 'bob-id', preferred_username: 'bob' } : undefined,
    });
    TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter(routes),
        provideFakeKeycloak(keycloak, createKeycloakEvent(KeycloakEventType.Ready)),
      ],
    });
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    return fixture;
  }

  function render(authenticated: boolean): HTMLElement {
    return create(authenticated).nativeElement as HTMLElement;
  }

  function loader(fixture: ComponentFixture<App>): Element | null {
    return (fixture.nativeElement as HTMLElement).querySelector(
      '[role="status"][aria-label="Loading Phone Book"]',
    );
  }

  async function navigate(fixture: ComponentFixture<App>, url: string): Promise<void> {
    await TestBed.inject(Router)
      .navigateByUrl(url)
      .catch(() => false);
    await fixture.whenStable();
  }

  it('shows the header for an authenticated user', () => {
    const element = render(true);

    expect(element.querySelector('app-header')).not.toBeNull();
    expect(element.textContent).toContain('bob');
  });

  it('hides the header for an anonymous user', () => {
    const element = render(false);

    expect(element.querySelector('app-header')).toBeNull();
  });

  it('renders the routed content inside the main landmark', () => {
    const element = render(true);

    expect(element.querySelector('main router-outlet')).not.toBeNull();
  });

  it('shows a loading indicator until the first navigation completes', async () => {
    const fixture = create(true);

    expect(loader(fixture)).not.toBeNull();

    await navigate(fixture, '/');

    expect(loader(fixture)).toBeNull();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Routed content');
  });

  it('keeps the loading indicator while a guard redirects away from the app', async () => {
    const fixture = create(false);

    await navigate(fixture, '/denied');

    expect(loader(fixture)).not.toBeNull();
  });

  it('hides the loading indicator when the first navigation fails', async () => {
    const fixture = create(true);

    await navigate(fixture, '/broken');

    expect(loader(fixture)).toBeNull();
  });
});
