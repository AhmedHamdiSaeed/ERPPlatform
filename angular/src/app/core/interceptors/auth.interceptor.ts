import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injector, inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService, TENANT_KEY } from '../services/auth.service';
import { SessionTimeoutService } from '../services/session-timeout.service';
import { REFRESH_TOKEN_KEY, TOKEN_KEY } from '../constants/session.constants';

/** Marks a request as already retried so a refresh loop can never happen. */
const RETRY_HEADER = 'X-Auth-Retry';

export const authInterceptorFn: HttpInterceptorFn = (req, next) => {
  const session = inject(SessionTimeoutService);
  const injector = inject(Injector);
  const token = typeof localStorage !== 'undefined' ? localStorage.getItem(TOKEN_KEY) : null;
  const tenant = typeof localStorage !== 'undefined' ? localStorage.getItem(TENANT_KEY) : null;

  // Never gate the login / token / refresh endpoints themselves.
  const isAuthEndpoint = req.url.includes('/api/auth/login') ||
                         req.url.includes('/api/auth/refresh') ||
                         req.url.includes('/connect/token') ||
                         req.url.includes('/connect/revocation');

  if (!isAuthEndpoint && session.isExpired()) {
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

  let headersConfig: Record<string, string> = {};

  if (token) {
    headersConfig['Authorization'] = `Bearer ${token}`;
  }

  if (tenant) {
    headersConfig['X-Tenant-Name'] = tenant;
    headersConfig['__tenant'] = tenant;
  }

  const authReq = req.clone({
    setHeaders: headersConfig
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      const canRefresh = !!(typeof localStorage !== 'undefined' && localStorage.getItem(REFRESH_TOKEN_KEY)) && !req.headers.has(RETRY_HEADER);
      if (!canRefresh) {
        session.expire('unauthorized');
        return throwError(() => error);
      }

      // Replay the request with a fresh access token using refresh token
      return from(injector.get(AuthService).refreshSession()).pipe(
        switchMap(refreshed => {
          if (!refreshed) {
            session.expire('unauthorized');
            return throwError(() => error);
          }

          const freshToken = localStorage.getItem(TOKEN_KEY);
          let retryHeaders: Record<string, string> = {
            [RETRY_HEADER]: '1'
          };
          if (freshToken) {
            retryHeaders['Authorization'] = `Bearer ${freshToken}`;
          }
          if (tenant) {
            retryHeaders['X-Tenant-Name'] = tenant;
            retryHeaders['__tenant'] = tenant;
          }

          return next(
            req.clone({
              setHeaders: retryHeaders
            })
          );
        })
      );
    })
  );
};
