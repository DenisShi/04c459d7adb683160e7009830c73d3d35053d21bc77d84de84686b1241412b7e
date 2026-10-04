export type Visibility = 'PERSONAL' | 'SHARED';

export type PhoneNumberScope = 'all' | 'personal' | 'shared';

export interface PhoneNumber {
  readonly id: string;
  readonly contactName: string;
  readonly number: string;
  readonly visibility: Visibility;
  readonly ownerUsername: string;
  readonly isOwnedByCurrentUser: boolean;
  readonly createdAt: string;
}

export interface CreatePhoneNumberRequest {
  readonly contactName: string;
  readonly number: string;
  readonly visibility: Visibility;
}

export type FieldErrors = Readonly<Partial<Record<keyof CreatePhoneNumberRequest, string>>>;
