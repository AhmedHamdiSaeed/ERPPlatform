import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface PlanDto {
  id: string;
  name: string;
  code: string;
  description: string;
  price: number;
  currency: string;
  billingPeriod: string;
  isActive: boolean;
  isPublic: boolean;
  displayOrder: number;
}

export interface SubscriptionDto {
  id: string;
  tenantId?: string;
  planId: string;
  planName: string;
  planCode: string;
  planPrice: number;
  status: 'Trial' | 'Active' | 'PastDue' | 'Suspended' | 'Cancelled' | 'Expired';
  startDate: string;
  endDate: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  autoRenew: boolean;
}

export interface FeatureAccessDto {
  code: string;
  name: string;
  enabled: boolean;
  limit: number | null;
  usage: number;
  remaining: number | null;
  unit: string;
  category: string;
}

export interface PlanFeatureDto {
  id: string;
  planId: string;
  featureId: string;
  featureCode: string;
  featureName: string;
  isEnabled: boolean;
  limitValue: number | null;
  limitType: string;
  unit: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionApiService extends ErpApiService {

  async getCurrentSubscription(): Promise<SubscriptionDto> {
    return this.get<SubscriptionDto>('subscription/current');
  }

  async getPlans(): Promise<PlanDto[]> {
    return this.get<PlanDto[]>('subscription/plans');
  }

  async changePlan(planId: string): Promise<SubscriptionDto> {
    return this.post<SubscriptionDto>(`subscription/change-plan/${planId}`, {});
  }

  async cancelSubscription(): Promise<void> {
    await this.post('subscription/cancel', {});
  }

  async resumeSubscription(): Promise<void> {
    await this.post('subscription/resume', {});
  }

  async getSubscriptionFeatures(): Promise<FeatureAccessDto[]> {
    return this.get<FeatureAccessDto[]>('subscription/features');
  }

  async getPlanFeatures(planId: string): Promise<PlanFeatureDto[]> {
    return this.get<PlanFeatureDto[]>(`plan/features/${planId}`);
  }

  async updatePlanFeatures(planId: string, features: Partial<PlanFeatureDto>[]): Promise<void> {
    await this.put(`plan/features/${planId}`, features);
  }
}
