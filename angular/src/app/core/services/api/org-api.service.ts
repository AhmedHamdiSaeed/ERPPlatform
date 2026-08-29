import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export interface Company {
  id: string;
  name: string;
  taxNumber: string;
  email: string;
  phone: string;
  address: string;
  country: string;
  currency: string;
  website: string;
  logoUrl?: string;
  isActive: boolean;
}

export interface Branch {
  id: string;
  companyId: string;
  companyName: string;
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  isHeadquarters: boolean;
  isActive: boolean;
}

export interface CostCenter {
  id: string;
  code: string;
  name: string;
  description: string;
  isActive: boolean;
}

export interface FiscalYear {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  isClosed: boolean;
}

export interface Currency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  exchangeRate: number;
  isBase: boolean;
  isActive: boolean;
}

export interface TaxConfig {
  id: string;
  name: string;
  rate: number;
  taxType: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface PaymentTerm {
  id: string;
  name: string;
  dueDays: number;
  discountDays: number;
  discountPercent: number;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class OrgApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  // Company
  getCompanies(): Promise<Company[]> {
    return this.getList<Company>('company');
  }
  createCompany(c: Partial<Company>): Promise<void> { return this.post('company', c); }
  updateCompany(id: string, c: Partial<Company>): Promise<void> { return this.put(`company/${id}`, c); }
  deleteCompany(id: string): Promise<void> { return this.delete(`company/${id}`); }

  // Branch
  getBranches(): Promise<Branch[]> {
    return this.getList<Branch>('branch');
  }
  createBranch(b: Partial<Branch>): Promise<void> { return this.post('branch', b); }
  updateBranch(id: string, b: Partial<Branch>): Promise<void> { return this.put(`branch/${id}`, b); }
  deleteBranch(id: string): Promise<void> { return this.delete(`branch/${id}`); }

  // CostCenter
  getCostCenters(): Promise<CostCenter[]> {
    return this.getList<CostCenter>('cost-center');
  }
  createCostCenter(cc: Partial<CostCenter>): Promise<void> { return this.post('cost-center', cc); }
  updateCostCenter(id: string, cc: Partial<CostCenter>): Promise<void> { return this.put(`cost-center/${id}`, cc); }
  deleteCostCenter(id: string): Promise<void> { return this.delete(`cost-center/${id}`); }

  // FiscalYear
  getFiscalYears(): Promise<FiscalYear[]> {
    return this.getList<FiscalYear>('fiscal-year');
  }
  createFiscalYear(fy: Partial<FiscalYear>): Promise<void> { return this.post('fiscal-year', fy); }
  setCurrentFiscalYear(id: string): Promise<void> { return this.post(`fiscal-year/${id}/set-current`, {}); }

  // Currency
  getCurrencies(): Promise<Currency[]> {
    return this.getList<Currency>('currency');
  }
  createCurrency(c: Partial<Currency>): Promise<void> { return this.post('currency', c); }
  updateCurrency(id: string, c: Partial<Currency>): Promise<void> { return this.put(`currency/${id}`, c); }
  deleteCurrency(id: string): Promise<void> { return this.delete(`currency/${id}`); }

  // TaxConfig
  getTaxConfigs(): Promise<TaxConfig[]> {
    return this.getList<TaxConfig>('tax-config');
  }
  createTaxConfig(t: Partial<TaxConfig>): Promise<void> { return this.post('tax-config', t); }
  updateTaxConfig(id: string, t: Partial<TaxConfig>): Promise<void> { return this.put(`tax-config/${id}`, t); }
  deleteTaxConfig(id: string): Promise<void> { return this.delete(`tax-config/${id}`); }

  // PaymentTerm
  getPaymentTerms(): Promise<PaymentTerm[]> {
    return this.getList<PaymentTerm>('payment-term');
  }
  createPaymentTerm(pt: Partial<PaymentTerm>): Promise<void> { return this.post('payment-term', pt); }
  updatePaymentTerm(id: string, pt: Partial<PaymentTerm>): Promise<void> { return this.put(`payment-term/${id}`, pt); }
  deletePaymentTerm(id: string): Promise<void> { return this.delete(`payment-term/${id}`); }
}
