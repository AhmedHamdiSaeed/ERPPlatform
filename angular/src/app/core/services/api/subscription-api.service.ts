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
    try {
      return await this.get<SubscriptionDto>('subscription/current');
    } catch {
      return {
        id: 'sub-001',
        planId: 'p-basic',
        planName: 'Basic Plan',
        planCode: 'BASIC',
        planPrice: 29,
        status: 'Active',
        startDate: '2026-08-01',
        endDate: '2026-08-31',
        currentPeriodStart: '2026-08-01',
        currentPeriodEnd: '2026-08-31',
        autoRenew: true
      };
    }
  }

  async getPlans(): Promise<PlanDto[]> {
    try {
      return await this.get<PlanDto[]>('subscription/plans');
    } catch {
      return [
        { id: '1', name: 'Free Plan', code: 'FREE', description: 'Starter tier for small business trials', price: 0, currency: 'USD', billingPeriod: 'Monthly', isActive: true, isPublic: true, displayOrder: 1 },
        { id: '2', name: 'Basic Plan', code: 'BASIC', description: 'Ideal for growing small teams', price: 29, currency: 'USD', billingPeriod: 'Monthly', isActive: true, isPublic: true, displayOrder: 2 },
        { id: '3', name: 'Professional Plan', code: 'PROFESSIONAL', description: 'Full feature suite for expanding enterprises', price: 99, currency: 'USD', billingPeriod: 'Monthly', isActive: true, isPublic: true, displayOrder: 3 },
        { id: '4', name: 'Enterprise Plan', code: 'ENTERPRISE', description: 'Unlimited corporate capability & priority SLA', price: 299, currency: 'USD', billingPeriod: 'Monthly', isActive: true, isPublic: true, displayOrder: 4 }
      ];
    }
  }

  async changePlan(planId: string): Promise<SubscriptionDto> {
    return await this.post<SubscriptionDto>('subscription/change-plan', { newPlanId: planId });
  }

  async cancelSubscription(): Promise<void> {
    await this.post('subscription/cancel', {});
  }

  async resumeSubscription(): Promise<void> {
    await this.post('subscription/resume', {});
  }

  async getSubscriptionFeatures(): Promise<FeatureAccessDto[]> {
    try {
      return await this.get<FeatureAccessDto[]>('subscription/features');
    } catch {
      return [
        { code: 'ERP.Invoices.Monthly', name: 'Monthly Invoices', enabled: true, limit: 100, usage: 73, remaining: 27, unit: 'per_month', category: 'Sales' },
        { code: 'ERP.Users.Max', name: 'Maximum Users', enabled: true, limit: 5, usage: 4, remaining: 1, unit: 'count', category: 'Admin' },
        { code: 'ERP.Branches.Max', name: 'Maximum Branches', enabled: true, limit: 2, usage: 1, remaining: 1, unit: 'count', category: 'Org' },
        { code: 'ERP.Employees.Max', name: 'Maximum Employees', enabled: true, limit: 50, usage: 18, remaining: 32, unit: 'count', category: 'HR' },
        { code: 'ERP.Quotations.Monthly', name: 'Monthly Quotations', enabled: true, limit: 100, usage: 42, remaining: 58, unit: 'per_month', category: 'Sales' },
        { code: 'ERP.Storage.MaxGB', name: 'Storage Allocation (GB)', enabled: true, limit: 5, usage: 2, remaining: 3, unit: 'GB', category: 'System' },
        { code: 'ERP.AdvancedReports', name: 'Advanced Reports & AI', enabled: false, limit: null, usage: 0, remaining: null, unit: 'boolean', category: 'Analytics' },
        { code: 'ERP.ApiAccess', name: 'REST API Access', enabled: false, limit: null, usage: 0, remaining: null, unit: 'boolean', category: 'System' },
        { code: 'ERP.MultiCurrency', name: 'Multi-Currency Engine', enabled: true, limit: null, usage: 0, remaining: null, unit: 'boolean', category: 'Finance' }
      ];
    }
  }

  async getPlanFeatures(planId: string): Promise<PlanFeatureDto[]> {
    try {
      return await this.get<PlanFeatureDto[]>(`plan/${planId}/features`);
    } catch {
      return [];
    }
  }

  async updatePlanFeatures(planId: string, features: Partial<PlanFeatureDto>[]): Promise<void> {
    await this.post(`plan/${planId}/features`, features);
  }
}
