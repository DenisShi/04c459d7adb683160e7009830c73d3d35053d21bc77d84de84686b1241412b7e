import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { TestBed } from '@angular/core/testing';
import { MatButtonHarness } from '@angular/material/button/testing';
import { KeycloakEventType } from 'keycloak-angular';
import {
  FakeKeycloak,
  createFakeKeycloak,
  createKeycloakEvent,
  provideFakeKeycloak,
} from '../../../testing/fake-keycloak';
import { Header } from './header';

describe('Header', () => {
  let keycloak: FakeKeycloak;

  beforeEach(() => {
    keycloak = createFakeKeycloak({
      authenticated: true,
      tokenParsed: { sub: 'alice-id', preferred_username: 'alice' },
    });
    TestBed.configureTestingModule({
      imports: [Header],
      providers: [
        provideFakeKeycloak(keycloak, createKeycloakEvent(KeycloakEventType.AuthSuccess)),
      ],
    });
  });

  it('shows the application title and the signed-in username', async () => {
    const fixture = TestBed.createComponent(Header);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Phone Book');
    expect(element.querySelector('[data-testid="current-username"]')?.textContent).toBe('alice');
  });

  it('signs out when the sign-out button is clicked', async () => {
    const fixture = TestBed.createComponent(Header);
    const loader = TestbedHarnessEnvironment.loader(fixture);

    const button = await loader.getHarness(MatButtonHarness.with({ text: 'Sign out' }));
    await button.click();

    expect(keycloak.logout).toHaveBeenCalledOnce();
  });
});
