import { FormControl } from '@angular/forms';
import cases from '../../../../../../contracts/phone-number-validation-cases.json';
import {
  contactNameValidator,
  normalizePhoneNumber,
  phoneNumberValidator,
  trimLikeServer,
} from './phone-number-validators';

describe('contactNameValidator', () => {
  it.each(cases.contactName.valid)('accepts "$input"', ({ input }) => {
    expect(contactNameValidator(new FormControl(input))).toBeNull();
  });

  it.each(cases.contactName.invalid)('rejects %j', (input) => {
    expect(contactNameValidator(new FormControl(input))).toEqual({ required: true });
  });

  it('accepts 100 characters and rejects 101', () => {
    expect(contactNameValidator(new FormControl('a'.repeat(100)))).toBeNull();
    expect(contactNameValidator(new FormControl('a'.repeat(101)))).toHaveProperty('maxlength');
  });
});

describe('phoneNumberValidator', () => {
  it.each(cases.number.valid)('accepts "$input"', ({ input }) => {
    expect(phoneNumberValidator(new FormControl(input))).toBeNull();
  });

  it.each(cases.number.invalid)('rejects %j', (input) => {
    expect(phoneNumberValidator(new FormControl(input))).not.toBeNull();
  });

  it('accepts 32 characters and rejects 33 after trimming', () => {
    const padded = (spaces: number) => '+420' + ' '.repeat(spaces) + '601234567';
    expect(phoneNumberValidator(new FormControl(` ${padded(19)} `))).toBeNull();
    expect(phoneNumberValidator(new FormControl(padded(20)))).toHaveProperty('maxlength');
  });

  it.each(['+420 60123456', '+420 6012345678'])(
    'rejects %j because its length does not fit the country numbering plan',
    (input) => {
      expect(phoneNumberValidator(new FormControl(input))).toEqual({ phoneNumber: true });
    },
  );

  it('rejects a national number without the country code', () => {
    expect(phoneNumberValidator(new FormControl('601 234 567'))).toEqual({ phoneNumber: true });
  });
});

describe('normalizePhoneNumber', () => {
  it.each(cases.number.valid)('normalizes "$input" to "$normalized"', ({ input, normalized }) => {
    expect(normalizePhoneNumber(input)).toBe(normalized);
  });

  it.each(cases.number.invalid)('returns null for %j', (input) => {
    expect(normalizePhoneNumber(input)).toBeNull();
  });
});

describe('trimLikeServer', () => {
  it('trims the same characters as the server', () => {
    expect(trimLikeServer('\u00a0\u2003 a b \u0085\t\n')).toBe('a b');
  });

  it('keeps the byte order mark that the server does not trim', () => {
    expect(trimLikeServer('\ufeffa\ufeff')).toBe('\ufeffa\ufeff');
  });
});
