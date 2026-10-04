import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const CONTACT_NAME_MAX_LENGTH = 100;
export const PHONE_NUMBER_MAX_INPUT_LENGTH = 32;

const NORMALIZED_PHONE_NUMBER = /^\+?[0-9]{3,15}$/;
const PHONE_NUMBER_SEPARATORS = /[ \-.()]/g;
const SERVER_WHITESPACE =
  '\\t-\\r \\u0085\\u00a0\\u1680\\u2000-\\u200a\\u2028\\u2029\\u202f\\u205f\\u3000';
const SURROUNDING_WHITESPACE = new RegExp(`^[${SERVER_WHITESPACE}]+|[${SERVER_WHITESPACE}]+$`, 'g');

export function trimLikeServer(input: string): string {
  return input.replace(SURROUNDING_WHITESPACE, '');
}

export function normalizePhoneNumber(input: string): string {
  return trimLikeServer(input).replace(PHONE_NUMBER_SEPARATORS, '');
}

export const contactNameValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = trimLikeServer(String(control.value ?? ''));
  if (value.length === 0) {
    return { required: true };
  }
  if (value.length > CONTACT_NAME_MAX_LENGTH) {
    return { maxlength: { requiredLength: CONTACT_NAME_MAX_LENGTH, actualLength: value.length } };
  }
  return null;
};

export const phoneNumberValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = trimLikeServer(String(control.value ?? ''));
  if (value.length === 0) {
    return { required: true };
  }
  if (value.length > PHONE_NUMBER_MAX_INPUT_LENGTH) {
    return {
      maxlength: { requiredLength: PHONE_NUMBER_MAX_INPUT_LENGTH, actualLength: value.length },
    };
  }
  if (!NORMALIZED_PHONE_NUMBER.test(normalizePhoneNumber(value))) {
    return { phoneNumber: true };
  }
  return null;
};
