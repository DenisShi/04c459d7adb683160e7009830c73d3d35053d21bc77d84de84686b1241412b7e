import { RuntimeConfigError } from './runtime-config';

export function renderStartupError(document: Document, error: unknown): void {
  const container = document.createElement('main');
  container.className = 'startup-error';
  container.setAttribute('role', 'alert');

  const heading = document.createElement('h1');
  heading.textContent = 'Phone Book could not start';

  const details = document.createElement('p');
  details.textContent =
    error instanceof RuntimeConfigError
      ? `The runtime configuration is invalid. ${error.message}`
      : 'An unexpected error occurred while starting the application. Reload the page to try again.';

  container.append(heading, details);
  document.body.replaceChildren(container);
}
