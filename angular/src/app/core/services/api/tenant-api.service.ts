import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import {
  TenantProfile,
  CreateTenantInput,
  UpdateTenantInput,
  TenantStatsSummary,
  TenantImpersonationResult
} from '../../models/erp-models';
import { environment } from '../../../../environments/environment';

const STORAGE_KEY = 'erp_saas_tenants_cache';

@Injectable({ providedIn: 'root' })
export class TenantApiService extends ErpApiService {
  private localTenants: TenantProfile[] = [];

  constructor() {
    super();
    this.initLocalCache();
  }

  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/app/tenant-management`;
  }

  private initLocalCache(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const saved = window.localStorage.getItem(STORAGE_KEY);
        if (saved) {
          this.localTenants = JSON.parse(saved);
          return;
        }
      } catch (e) {
        console.warn('Could not read tenants cache from localStorage', e);
      }
    }
    this.localTenants = this.getFallbackTenants();
    this.persistLocalCache();
  }

  private persistLocalCache(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(this.localTenants));
      } catch (e) {
        console.warn('Could not persist tenants cache to localStorage', e);
      }
    }
  }

  async getTenants(filter?: string, status?: string, planTier?: string): Promise<TenantProfile[]> {
    const params = new URLSearchParams();
    if (filter) params.append('filter', filter);
    if (status && status !== 'ALL') params.append('status', status);
    if (planTier && planTier !== 'ALL') params.append('planTier', planTier);

    const query = params.toString() ? `?${params.toString()}` : '';
    try {
      const data = await this.get<TenantProfile[]>(`tenant-list${query}`);
      if (data && data.length > 0) {
        this.localTenants = data;
        this.persistLocalCache();
        return data;
      }
      return [...this.localTenants];
    } catch {
      return [...this.localTenants];
    }
  }

  async getTenant(id: string): Promise<TenantProfile> {
    try {
      return await this.get<TenantProfile>(`tenant/${id}`);
    } catch {
      const found = this.localTenants.find(t => t.id === id || t.tenantId === id);
      if (found) return found;
      throw new Error(`Tenant with ID ${id} not found.`);
    }
  }

  async createTenant(input: CreateTenantInput): Promise<TenantProfile> {
    try {
      const created = await this.post<TenantProfile>('create-tenant', input);
      this.localTenants.unshift(created);
      this.persistLocalCache();
      return created;
    } catch {
      // Fallback local create
      const newTenant: TenantProfile = {
        id: `t-${Date.now()}`,
        tenantId: `00000000-0000-0000-0000-${Date.now().toString(16).padStart(12, '0')}`,
        code: input.subdomain.toLowerCase().trim(),
        name: input.name,
        legalName: input.legalName || input.name,
        subdomain: input.subdomain.toLowerCase().trim(),
        taxNumber: input.taxNumber || '',
        currency: input.currency || 'SAR',
        timezone: input.timezone || 'Asia/Riyadh',
        logoUrl: input.logoUrl || 'https://images.unsplash.com/photo-1572021335469-31706a17aaef?w=150',
        planTier: input.planTier || 'Starter',
        status: 'Active',
        maxUsers: input.maxUsers || 10,
        storageLimitGb: input.storageLimitGb || 10,
        usedStorageMb: 0,
        activeUserCount: 1,
        adminEmail: input.adminEmail,
        adminFullName: input.adminFullName,
        adminPhone: input.adminPhone,
        isDedicatedDb: input.isDedicatedDb ?? false,
        customConnectionString: input.customConnectionString,
        enabledModules: input.enabledModules || ['Sales', 'Inventory', 'Finance'],
        monthlyFee: input.planTier === 'Enterprise' ? 899 : input.planTier === 'Professional' ? 299 : input.planTier === 'Trial' ? 0 : 99,
        creationTime: new Date().toISOString()
      };
      this.localTenants.unshift(newTenant);
      this.persistLocalCache();
      return newTenant;
    }
  }

  async updateTenant(id: string, input: UpdateTenantInput): Promise<TenantProfile> {
    try {
      const updated = await this.put<TenantProfile>(`tenant/${id}`, input);
      const idx = this.localTenants.findIndex(t => t.id === id || t.tenantId === id);
      if (idx >= 0) {
        this.localTenants[idx] = updated;
        this.persistLocalCache();
      }
      return updated;
    } catch {
      const idx = this.localTenants.findIndex(t => t.id === id || t.tenantId === id);
      if (idx >= 0) {
        const current = this.localTenants[idx];
        const updated: TenantProfile = {
          ...current,
          name: input.name ?? current.name,
          legalName: input.legalName ?? current.legalName,
          taxNumber: input.taxNumber ?? current.taxNumber,
          currency: input.currency ?? current.currency,
          timezone: input.timezone ?? current.timezone,
          logoUrl: input.logoUrl ?? current.logoUrl,
          planTier: input.planTier ?? current.planTier,
          status: input.status ?? current.status,
          maxUsers: input.maxUsers ?? current.maxUsers,
          storageLimitGb: input.storageLimitGb ?? current.storageLimitGb,
          isDedicatedDb: input.isDedicatedDb ?? current.isDedicatedDb,
          customConnectionString: input.customConnectionString ?? current.customConnectionString,
          enabledModules: input.enabledModules ?? current.enabledModules
        };
        this.localTenants[idx] = updated;
        this.persistLocalCache();
        return updated;
      }
      throw new Error(`Tenant ${id} not found.`);
    }
  }

  async deleteTenant(id: string): Promise<void> {
    try {
      await this.delete(`tenant/${id}`);
    } catch {
      // Local fallback removal
    }
    this.localTenants = this.localTenants.filter(t => t.id !== id && t.tenantId !== id);
    this.persistLocalCache();
  }

  async setStatus(id: string, status: string): Promise<void> {
    try {
      await this.post(`tenant/${id}/set-status?status=${encodeURIComponent(status)}`, {});
    } catch {
      // Local fallback
    }
    const target = this.localTenants.find(t => t.id === id || t.tenantId === id);
    if (target) {
      target.status = status;
      this.persistLocalCache();
    }
  }

  async setConnectionString(id: string, connectionString: string): Promise<void> {
    try {
      await this.post(`tenant/${id}/set-connection-string?connectionString=${encodeURIComponent(connectionString)}`, {});
    } catch {
      // Local fallback
    }
    const target = this.localTenants.find(t => t.id === id || t.tenantId === id);
    if (target) {
      target.customConnectionString = connectionString;
      target.isDedicatedDb = !!connectionString;
      this.persistLocalCache();
    }
  }

  async toggleModule(id: string, moduleName: string, isEnabled: boolean): Promise<void> {
    try {
      await this.post(`tenant/${id}/toggle-module?moduleName=${encodeURIComponent(moduleName)}&isEnabled=${isEnabled}`, {});
    } catch {
      // Local fallback
    }
    const target = this.localTenants.find(t => t.id === id || t.tenantId === id);
    if (target) {
      if (isEnabled && !target.enabledModules.includes(moduleName)) {
        target.enabledModules.push(moduleName);
      } else if (!isEnabled) {
        target.enabledModules = target.enabledModules.filter(m => m.toLowerCase() !== moduleName.toLowerCase());
      }
      this.persistLocalCache();
    }
  }

  async impersonateTenant(id: string): Promise<TenantImpersonationResult> {
    try {
      return await this.post<TenantImpersonationResult>(`tenant/${id}/impersonate`, {});
    } catch {
      const target = this.localTenants.find(t => t.id === id || t.tenantId === id);
      return {
        success: true,
        impersonationToken: 'mock_impersonation_jwt_token_' + id,
        accessToken: 'mock_impersonation_jwt_token_' + id,
        tenantId: target?.tenantId || id,
        tenantName: target?.name || 'Tenant',
        redirectUrl: `/dashboard?tenantId=${target?.subdomain || id}`
      };
    }
  }

  async getStatsSummary(): Promise<TenantStatsSummary> {
    try {
      return await this.get<TenantStatsSummary>('stats-summary');
    } catch {
      const list = this.localTenants;
      const totalTenants = list.length;
      const activeTenants = list.filter(t => t.status === 'Active').length;
      const trialTenants = list.filter(t => t.status === 'Trial').length;
      const suspendedTenants = list.filter(t => t.status === 'Suspended').length;
      const totalMRR = list.filter(t => t.status === 'Active' || t.status === 'Trial').reduce((acc, t) => acc + (t.monthlyFee || 0), 0);
      const totalUsersAcrossTenants = list.reduce((acc, t) => acc + (t.activeUserCount || 0), 0);
      const totalStorageAllocatedGb = list.reduce((acc, t) => acc + (t.storageLimitGb || 0), 0);

      return {
        totalTenants,
        activeTenants,
        trialTenants,
        suspendedTenants,
        totalMRR,
        totalUsersAcrossTenants,
        totalStorageAllocatedGb
      };
    }
  }

  private getFallbackTenants(): TenantProfile[] {
    return [
      {
        id: 't-1',
        tenantId: 'd3b07384-d113-4f36-a36c-9c92257211f1',
        code: 'al-madina-trading',
        name: 'Al-Madina Global Trading',
        legalName: 'Al-Madina Global Trading LLC',
        subdomain: 'almadina',
        taxNumber: 'VAT-99281-SA',
        currency: 'SAR',
        timezone: 'Asia/Riyadh',
        logoUrl: 'https://images.unsplash.com/photo-1572021335469-31706a17aaef?w=150',
        primaryColor: '#059669',
        planTier: 'Enterprise',
        status: 'Active',
        maxUsers: 150,
        storageLimitGb: 100,
        usedStorageMb: 14200.0,
        activeUserCount: 48,
        adminEmail: 'admin@almadina-trading.com',
        adminFullName: 'Tariq Al-Mansoor',
        adminPhone: '+966 50 123 4567',
        isDedicatedDb: true,
        customConnectionString: 'Server=db-dedicated-01.internal;Database=ERP_AlMadina;User Id=erp_user;Password=***;',
        enabledModules: ['HR', 'Finance', 'Sales', 'Inventory', 'Manufacturing', 'AI', 'Workflow'],
        monthlyFee: 899,
        creationTime: '2026-01-15'
      },
      {
        id: 't-2',
        tenantId: 'e4c18495-e224-4f47-b47d-0d03368322e2',
        code: 'apex-tech-solutions',
        name: 'Apex Technology Solutions',
        legalName: 'Apex Solutions FZCO',
        subdomain: 'apex',
        taxNumber: 'AE-8840192',
        currency: 'USD',
        timezone: 'Asia/Dubai',
        logoUrl: 'https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=150',
        primaryColor: '#4f46e5',
        planTier: 'Professional',
        status: 'Active',
        maxUsers: 50,
        storageLimitGb: 30,
        usedStorageMb: 4850.0,
        activeUserCount: 22,
        adminEmail: 'sysadmin@apexsolutions.io',
        adminFullName: 'Laila Mahmoud',
        adminPhone: '+971 4 999 8888',
        isDedicatedDb: false,
        enabledModules: ['HR', 'Finance', 'Sales', 'Inventory', 'AI', 'Workflow'],
        monthlyFee: 299,
        creationTime: '2026-03-01'
      },
      {
        id: 't-3',
        tenantId: 'f5d29506-f335-4f58-c58e-1e14479433f3',
        code: 'cairo-logistics-hub',
        name: 'Cairo Express Logistics',
        legalName: 'Cairo Express Logistics S.A.E.',
        subdomain: 'cairoexpress',
        taxNumber: 'EG-TR-44819',
        currency: 'EGP',
        timezone: 'Africa/Cairo',
        logoUrl: 'https://images.unsplash.com/photo-1542744173-8e7e53415bb0?w=150',
        primaryColor: '#d97706',
        planTier: 'Starter',
        status: 'Trial',
        maxUsers: 15,
        storageLimitGb: 10,
        usedStorageMb: 820.0,
        activeUserCount: 5,
        adminEmail: 'karim@cairoexpress.eg',
        adminFullName: 'Karim El-Sayed',
        adminPhone: '+20 100 555 1234',
        isDedicatedDb: false,
        enabledModules: ['Finance', 'Sales', 'Inventory', 'Workflow'],
        trialEndDate: '2026-10-05',
        monthlyFee: 99,
        creationTime: '2026-09-20'
      }
    ];
  }
}
