import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { SessionTimeoutService } from '../services/session-timeout.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const session = inject(SessionTimeoutService);

  if (authService.isAuthenticated()) {
    session.registerActivity();
    return true;
  }

  // No valid session: remember the requested page and send the user to login.
  // After signing in again the login component navigates back to `returnUrl`.
  const hadToken = !!authService.getToken();
  return session.redirectToLogin(state.url, hadToken ? 'timeout' : undefined);
};
