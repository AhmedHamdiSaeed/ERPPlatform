import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SubscriptionApiService } from '../services/api/subscription-api.service';

export const featureGuard = (requiredFeatureCode: string): CanActivateFn => {
  return async () => {
    const subscriptionApi = inject(SubscriptionApiService);
    const router = inject(Router);

    const features = await subscriptionApi.getSubscriptionFeatures();
    const feat = features.find(f => f.code === requiredFeatureCode);

    if (feat && feat.enabled) {
      return true;
    }

    return router.createUrlTree(['/saas/subscription']);
  };
};
