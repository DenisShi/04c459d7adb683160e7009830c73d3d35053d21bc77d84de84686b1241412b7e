import { IncludeBearerTokenCondition, createInterceptorCondition } from 'keycloak-angular';

export const API_URL_PATTERN = /^\/api(\/|$)/;

export function isApiUrl(url: string): boolean {
  return API_URL_PATTERN.test(url);
}

export const apiBearerTokenCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: API_URL_PATTERN,
  bearerPrefix: 'Bearer',
});
