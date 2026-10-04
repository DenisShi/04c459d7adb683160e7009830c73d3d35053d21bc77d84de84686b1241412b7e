import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { createAppConfig } from './app/app.config';
import { loadRuntimeConfig } from './app/core/config/runtime-config';
import { renderStartupError } from './app/core/config/startup-error';

loadRuntimeConfig()
  .then((config) => bootstrapApplication(App, createAppConfig(config, window.location.origin)))
  .catch((error: unknown) => {
    console.error(error);
    renderStartupError(document, error);
  });
