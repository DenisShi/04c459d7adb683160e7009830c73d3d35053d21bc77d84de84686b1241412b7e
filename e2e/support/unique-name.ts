import { randomBytes } from 'node:crypto';

export function uniqueContactName(label: string): string {
  return `E2E ${label} ${Date.now()}-${randomBytes(2).toString('hex')}`;
}
