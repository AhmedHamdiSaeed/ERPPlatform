import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StateService } from '../services/state.service';
import { AuthService } from '../services/auth.service';

export const permissionGuard = (requiredPermission: string): CanActivateFn => {
  return (route, stateSnapshot) => {
    const state = inject(StateService);
    const auth = inject(AuthService);
    const router = inject(Router);

    // If not authenticated, always send to login
    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/auth/login'], { queryParams: { returnUrl: stateSnapshot.url } });
    }

    // If user has permission or is Admin with wildcard
    if (state.hasPermission(requiredPermission)) {
      return true;
    }

    const targetUrl = stateSnapshot.url;
    // Allow dashboard access for all authenticated users
    if (targetUrl === '/dashboard' || targetUrl === '/' || targetUrl.startsWith('/dashboard')) {
      return true;
    }

    // Default redirect to dashboard if specific sub-page is unauthorized
    return router.createUrlTree(['/dashboard']);
  };
};
