import path from 'node:path';

export interface TestUser {
  readonly username: string;
  readonly password: string;
  readonly storageStatePath: string;
}

const authDirectory = path.resolve(__dirname, '..', '.auth');

function createUser(key: string, defaultCredential: string): TestUser {
  const prefix = `E2E_${key.toUpperCase()}`;
  return {
    username: process.env[`${prefix}_USERNAME`] ?? defaultCredential,
    password: process.env[`${prefix}_PASSWORD`] ?? defaultCredential,
    storageStatePath: path.join(authDirectory, `${key}.json`),
  };
}

export const alice = createUser('alice', 'alice');
export const bob = createUser('bob', 'bob');
export const signedInUsers: readonly TestUser[] = [alice, bob];
