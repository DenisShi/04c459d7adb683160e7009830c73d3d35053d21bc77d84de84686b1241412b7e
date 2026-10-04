import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import {
  CreatePhoneNumberRequest,
  FieldErrors,
  PhoneNumber,
  PhoneNumberScope,
} from './phone-number';
import { PhoneNumbersApi } from './phone-numbers-api';

const FIELD_NAMES: readonly (keyof CreatePhoneNumberRequest)[] = [
  'contactName',
  'number',
  'visibility',
];

const EMPTY_LIST: readonly PhoneNumber[] = [];

export function extractFieldErrors(error: unknown): FieldErrors {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) {
    return {};
  }
  const errors: unknown = error.error?.errors;
  if (typeof errors !== 'object' || errors === null) {
    return {};
  }
  const lookup = errors as Record<string, unknown>;
  const result: Partial<Record<keyof CreatePhoneNumberRequest, string>> = {};
  for (const field of FIELD_NAMES) {
    const messages = lookup[field];
    if (Array.isArray(messages) && typeof messages[0] === 'string') {
      result[field] = messages[0];
    }
  }
  return result;
}

@Injectable()
export class PhoneNumbersStore {
  private readonly api = inject(PhoneNumbersApi);

  readonly scope = signal<PhoneNumberScope>('all');
  readonly saving = signal(false);
  readonly fieldErrors = signal<FieldErrors>({});
  readonly saveFailed = signal(false);

  private readonly list = rxResource({
    params: () => this.scope(),
    stream: ({ params }) => this.api.list(params),
  });

  readonly phoneNumbers = computed(() => (this.list.hasValue() ? this.list.value() : EMPTY_LIST));
  readonly loading = computed(() => this.list.isLoading());
  readonly loadFailed = computed(() => this.list.error() !== undefined);

  selectScope(scope: PhoneNumberScope): void {
    this.scope.set(scope);
  }

  reload(): void {
    this.list.reload();
  }

  async create(request: CreatePhoneNumberRequest): Promise<boolean> {
    this.saving.set(true);
    this.fieldErrors.set({});
    this.saveFailed.set(false);
    try {
      await firstValueFrom(this.api.create(request));
      this.list.reload();
      return true;
    } catch (error) {
      const fieldErrors = extractFieldErrors(error);
      this.fieldErrors.set(fieldErrors);
      this.saveFailed.set(Object.keys(fieldErrors).length === 0);
      return false;
    } finally {
      this.saving.set(false);
    }
  }
}
