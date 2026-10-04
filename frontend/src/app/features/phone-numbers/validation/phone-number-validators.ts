import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const CONTACT_NAME_MAX_LENGTH = 100;
export const PHONE_NUMBER_MAX_INPUT_LENGTH = 32;

const NORMALIZED_PHONE_NUMBER = /^\+?[0-9]{3,15}$/;
const PHONE_NUMBER_SEPARATORS = /[ \-.()]/g;

export function normalizePhoneNumber(input: string): string {
  return input.trim().replace(PHONE_NUMBER_SEPARATORS, '');
}

export const contactNameValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = String(control.value ?? '').trim();
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
  const value = String(control.value ?? '').trim();
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
