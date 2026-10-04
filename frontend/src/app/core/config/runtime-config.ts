export interface KeycloakRuntimeConfig {
  readonly url: string;
  readonly realm: string;
  readonly clientId: string;
}

export interface RuntimeConfig {
  readonly keycloak: KeycloakRuntimeConfig;
}

export const RUNTIME_CONFIG_URL = '/config.json';

export class RuntimeConfigError extends Error {
  override readonly name = 'RuntimeConfigError';
}

export async function loadRuntimeConfig(fetchFn: typeof fetch = fetch): Promise<RuntimeConfig> {
  const response = await fetchFn(RUNTIME_CONFIG_URL, { cache: 'no-store' });
  if (!response.ok) {
    throw new RuntimeConfigError(`Failed to load ${RUNTIME_CONFIG_URL}: HTTP ${response.status}.`);
  }
  return parseRuntimeConfig(await response.json());
}

export function parseRuntimeConfig(value: unknown): RuntimeConfig {
  if (!isRecord(value) || !isRecord(value['keycloak'])) {
    throw new RuntimeConfigError('The runtime configuration must contain a "keycloak" object.');
  }

  const keycloak = value['keycloak'];
  const url = requireNonEmptyString(keycloak, 'url');
  const realm = requireNonEmptyString(keycloak, 'realm');
  const clientId = requireNonEmptyString(keycloak, 'clientId');

  if (!isAbsoluteHttpUrl(url)) {
    throw new RuntimeConfigError('"keycloak.url" must be an absolute http or https URL.');
  }

  return { keycloak: { url, realm, clientId } };
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function requireNonEmptyString(source: Record<string, unknown>, key: string): string {
  const value = source[key];
  if (typeof value !== 'string' || value.trim() === '') {
    throw new RuntimeConfigError(`"keycloak.${key}" must be a non-empty string.`);
  }
  return value.trim();
}

function isAbsoluteHttpUrl(value: string): boolean {
  if (!URL.canParse(value)) {
    return false;
  }
  const { protocol } = new URL(value);
  return protocol === 'http:' || protocol === 'https:';
}
