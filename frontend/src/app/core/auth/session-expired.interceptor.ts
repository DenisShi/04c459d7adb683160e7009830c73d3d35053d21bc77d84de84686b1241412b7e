import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { DOCUMENT, inject } from '@angular/core';
import Keycloak from 'keycloak-js';
import { catchError, throwError } from 'rxjs';
import { isApiUrl } from './api-url';

export const sessionExpiredInterceptor: HttpInterceptorFn = (request, next) => {
  const keycloak = inject(Keycloak);
  const document = inject(DOCUMENT);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === HttpStatusCode.Unauthorized &&
        isApiUrl(request.url)
      ) {
        void keycloak.login({ redirectUri: document.location.href });
      }
      return throwError(() => error);
    }),
  );
};
