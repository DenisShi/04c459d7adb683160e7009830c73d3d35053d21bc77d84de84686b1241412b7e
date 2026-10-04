import {
  RUNTIME_CONFIG_URL,
  RuntimeConfigError,
  loadRuntimeConfig,
  parseRuntimeConfig,
} from './runtime-config';

const validConfig = {
  keycloak: {
    url: 'http://localhost:8080',
    realm: 'phonebook',
    clientId: 'phonebook-spa',
  },
};

function withKeycloak(overrides: Record<string, unknown>): unknown {
  return { keycloak: { ...validConfig.keycloak, ...overrides } };
}

describe('parseRuntimeConfig', () => {
  it('accepts a valid configuration', () => {
    expect(parseRuntimeConfig(validConfig)).toEqual(validConfig);
  });

  it('accepts an https Keycloak URL', () => {
    const config = parseRuntimeConfig(withKeycloak({ url: 'https://id.example.com/auth' }));

    expect(config.keycloak.url).toBe('https://id.example.com/auth');
  });

  it.each([null, undefined, 'text', 42, [], {}, { keycloak: null }, { keycloak: [] }])(
    'rejects %j without a keycloak object',
    (value) => {
      expect(() => parseRuntimeConfig(value)).toThrow(RuntimeConfigError);
    },
  );

  it.each(['url', 'realm', 'clientId'])('rejects a missing %s', (key) => {
    const keycloak: Record<string, unknown> = { ...validConfig.keycloak };
    delete keycloak[key];

    expect(() => parseRuntimeConfig({ keycloak })).toThrow(`"keycloak.${key}"`);
  });

  it.each([
    ['url', ''],
    ['realm', '   '],
    ['clientId', ''],
    ['realm', 7],
    ['clientId', true],
  ])('rejects %s with value %j', (key, value) => {
    expect(() => parseRuntimeConfig(withKeycloak({ [key]: value }))).toThrow(RuntimeConfigError);
  });

  it.each(['/auth', 'localhost:8080', 'ftp://localhost:8080', 'javascript:alert(1)'])(
    'rejects the non-absolute or non-http URL %s',
    (url) => {
      expect(() => parseRuntimeConfig(withKeycloak({ url }))).toThrow('absolute http or https URL');
    },
  );
});

describe('loadRuntimeConfig', () => {
  it('fetches the configuration without caching and validates it', async () => {
    const fetchFn = vi.fn<typeof fetch>().mockResolvedValue(Response.json(validConfig));

    await expect(loadRuntimeConfig(fetchFn)).resolves.toEqual(validConfig);
    expect(fetchFn).toHaveBeenCalledWith(RUNTIME_CONFIG_URL, { cache: 'no-store' });
  });

  it('rejects when the server responds with an error status', async () => {
    const fetchFn = vi
      .fn<typeof fetch>()
      .mockResolvedValue(new Response('Not found', { status: 404 }));

    await expect(loadRuntimeConfig(fetchFn)).rejects.toThrow('HTTP 404');
  });

  it('rejects an invalid configuration body', async () => {
    const fetchFn = vi
      .fn<typeof fetch>()
      .mockResolvedValue(Response.json(withKeycloak({ url: '/relative' })));

    await expect(loadRuntimeConfig(fetchFn)).rejects.toThrow(RuntimeConfigError);
  });
});
