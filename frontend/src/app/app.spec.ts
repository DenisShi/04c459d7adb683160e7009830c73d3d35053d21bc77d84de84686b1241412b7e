import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { KeycloakEventType } from 'keycloak-angular';
import {
  createFakeKeycloak,
  createKeycloakEvent,
  provideFakeKeycloak,
} from '../testing/fake-keycloak';
import { App } from './app';

describe('App', () => {
  function render(authenticated: boolean): HTMLElement {
    const keycloak = createFakeKeycloak({
      authenticated,
      tokenParsed: authenticated ? { sub: 'bob-id', preferred_username: 'bob' } : undefined,
    });
    TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideFakeKeycloak(keycloak, createKeycloakEvent(KeycloakEventType.Ready)),
      ],
    });
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
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
});
