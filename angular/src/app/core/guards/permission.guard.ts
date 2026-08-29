import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StateService } from '../services/state.service';

export const permissionGuard = (requiredPermission: string): CanActivateFn => {
  return () => {
    const state = inject(StateService);
    const router = inject(Router);

    if (state.hasPermission(requiredPermission)) {
      return true;
    }

    return router.createUrlTree(['/dashboard']);
  };
};
