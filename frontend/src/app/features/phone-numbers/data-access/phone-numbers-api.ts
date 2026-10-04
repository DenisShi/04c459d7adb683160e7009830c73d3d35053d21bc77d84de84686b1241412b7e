import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PhoneNumber, PhoneNumberScope } from './phone-number';

export const PHONE_NUMBERS_URL = '/api/phone-numbers';

@Injectable({ providedIn: 'root' })
export class PhoneNumbersApi {
  private readonly http = inject(HttpClient);

  list(scope: PhoneNumberScope): Observable<readonly PhoneNumber[]> {
    return this.http.get<readonly PhoneNumber[]>(PHONE_NUMBERS_URL, { params: { scope } });
  }
}
