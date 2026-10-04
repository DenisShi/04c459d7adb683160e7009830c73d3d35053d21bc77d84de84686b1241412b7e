import { RuntimeConfigError } from './runtime-config';
import { renderStartupError } from './startup-error';

describe('renderStartupError', () => {
  beforeEach(() => {
    document.body.innerHTML = '<app-root></app-root>';
  });

  it('replaces the page with an alert describing a configuration error', () => {
    renderStartupError(document, new RuntimeConfigError('"keycloak.url" is missing.'));

    const alert = document.querySelector('[role="alert"]');
    expect(document.querySelector('app-root')).toBeNull();
    expect(alert?.querySelector('h1')?.textContent).toBe('Phone Book could not start');
    expect(alert?.textContent).toContain('"keycloak.url" is missing.');
  });

  it('shows a generic message for unexpected errors', () => {
    renderStartupError(document, new Error('<b>internal</b>'));

    const alert = document.querySelector('[role="alert"]');
    expect(alert?.textContent).toContain('An unexpected error occurred');
    expect(alert?.textContent).not.toContain('internal');
  });

  it('renders error text as plain text', () => {
    renderStartupError(document, new RuntimeConfigError('<img src=x onerror=alert(1)>'));

    expect(document.querySelector('img')).toBeNull();
  });
});
