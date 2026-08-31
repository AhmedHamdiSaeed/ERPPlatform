import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injector, inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { SessionTimeoutService } from '../services/session-timeout.service';
import { REFRESH_TOKEN_KEY, TOKEN_KEY } from '../constants/session.constants';

/** Marks a request as already retried so a refresh loop can never happen. */
const RETRY_HEADER = 'X-Auth-Retry';

export const authInterceptorFn: HttpInterceptorFn = (req, next) => {
  const session = inject(SessionTimeoutService);
  // Resolved lazily: AuthService needs HttpClient, which is still being built here.
  const injector = inject(Injector);
  const token = localStorage.getItem(TOKEN_KEY);

  // Never gate the token / revocation endpoints themselves.
  const isAuthRequest = req.url.includes('/connect/token') || req.url.includes('/connect/revocation');

  // Any other request fired after the session lifetime is over is stopped here
  // and the user is bounced to the login page (keeping the current page as returnUrl).
  if (!isAuthRequest && session.isExpired()) {
    session.expire('timeout');
    return throwError(
      () =>
        new HttpErrorResponse({
          status: 401,
          statusText: 'Session expired',
          url: req.url
        })
    );
  }

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthRequest) {
        return throwError(() => error);
      }

      const canRefresh = !!localStorage.getItem(REFRESH_TOKEN_KEY) && !req.headers.has(RETRY_HEADER);
      if (!canRefresh) {
        // No refresh token (demo mode) or already retried once -> session really is over.
        session.expire('unauthorized');
        return throwError(() => error);
      }

      // The server access token is short lived; renew it and replay the request once.
      // This is what keeps a mobile session usable for the full 6 months.
      return from(injector.get(AuthService).refreshSession()).pipe(
        switchMap(refreshed => {
          if (!refreshed) {
            session.expire('unauthorized');
            return throwError(() => error);
          }

          const freshToken = localStorage.getItem(TOKEN_KEY);
          return next(
            req.clone({
              setHeaders: {
                Authorization: `Bearer ${freshToken}`,
                [RETRY_HEADER]: '1'
              }
            })
          );
        })
      );
    })
  );
};
