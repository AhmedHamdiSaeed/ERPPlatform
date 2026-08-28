import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

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
  // Company
  getCompanies(): Promise<Company[]> {
    return this.getList<Company>('company').catch(() => [
      { id: 'cmp-1', name: 'ERP Enterprise Group HQ', taxNumber: 'EG-9021882', email: 'contact@erp.com', phone: '+20 2 2790 0000', address: 'Smart Village, Building B12', country: 'Egypt', currency: 'USD', website: 'https://erpplatform.com', isActive: true }
    ]);
  }
  createCompany(c: Partial<Company>): Promise<void> { return this.post('company', c); }
  updateCompany(id: string, c: Partial<Company>): Promise<void> { return this.put(`company/${id}`, c); }
  deleteCompany(id: string): Promise<void> { return this.delete(`company/${id}`); }

  // Branch
  getBranches(): Promise<Branch[]> {
    return this.getList<Branch>('branch').catch(() => [
      { id: 'br-1', companyId: 'cmp-1', companyName: 'ERP Enterprise Group HQ', name: 'Cairo Headquarters', code: 'BR-CAI', address: 'Downtown Cairo', phone: '+20 2 2500 1111', email: 'cairo@erp.com', isHeadquarters: true, isActive: true },
      { id: 'br-2', companyId: 'cmp-1', companyName: 'ERP Enterprise Group HQ', name: 'Alexandria Logistics Hub', code: 'BR-ALX', address: 'Free Trade Zone, Alexandria', phone: '+20 3 4800 2222', email: 'alex@erp.com', isHeadquarters: false, isActive: true }
    ]);
  }
  createBranch(b: Partial<Branch>): Promise<void> { return this.post('branch', b); }
  updateBranch(id: string, b: Partial<Branch>): Promise<void> { return this.put(`branch/${id}`, b); }
  deleteBranch(id: string): Promise<void> { return this.delete(`branch/${id}`); }

  // CostCenter
  getCostCenters(): Promise<CostCenter[]> {
    return this.getList<CostCenter>('cost-center').catch(() => [
      { id: 'cc-1', code: 'CC-ENG', name: 'Software Development & IT', description: 'R&D infrastructure & Cloud ops', isActive: true },
      { id: 'cc-2', code: 'CC-MKT', name: 'Global Sales & Advertising', description: 'Ad campaigns & client acquisition', isActive: true }
    ]);
  }
  createCostCenter(cc: Partial<CostCenter>): Promise<void> { return this.post('cost-center', cc); }
  updateCostCenter(id: string, cc: Partial<CostCenter>): Promise<void> { return this.put(`cost-center/${id}`, cc); }
  deleteCostCenter(id: string): Promise<void> { return this.delete(`cost-center/${id}`); }

  // FiscalYear
  getFiscalYears(): Promise<FiscalYear[]> {
    return this.getList<FiscalYear>('fiscal-year').catch(() => [
      { id: 'fy-2026', name: 'FY 2026', startDate: '2026-01-01', endDate: '2026-12-31', isCurrent: true, isClosed: false },
      { id: 'fy-2025', name: 'FY 2025', startDate: '2025-01-01', endDate: '2025-12-31', isCurrent: false, isClosed: true }
    ]);
  }
  createFiscalYear(fy: Partial<FiscalYear>): Promise<void> { return this.post('fiscal-year', fy); }
  setCurrentFiscalYear(id: string): Promise<void> { return this.post(`fiscal-year/${id}/set-current`, {}); }

  // Currency
  getCurrencies(): Promise<Currency[]> {
    return this.getList<Currency>('currency').catch(() => [
      { id: 'cur-1', code: 'USD', name: 'US Dollar', symbol: '$', exchangeRate: 1.0, isBase: true, isActive: true },
      { id: 'cur-2', code: 'EGP', name: 'Egyptian Pound', symbol: 'EGP', exchangeRate: 48.5, isBase: false, isActive: true },
      { id: 'cur-3', code: 'EUR', name: 'Euro', symbol: '€', exchangeRate: 0.92, isBase: false, isActive: true }
    ]);
  }
  createCurrency(c: Partial<Currency>): Promise<void> { return this.post('currency', c); }
  updateCurrency(id: string, c: Partial<Currency>): Promise<void> { return this.put(`currency/${id}`, c); }
  deleteCurrency(id: string): Promise<void> { return this.delete(`currency/${id}`); }

  // TaxConfig
  getTaxConfigs(): Promise<TaxConfig[]> {
    return this.getList<TaxConfig>('tax-config').catch(() => [
      { id: 'tax-1', name: 'Standard VAT', rate: 14.0, taxType: 'VAT', isDefault: true, isActive: true },
      { id: 'tax-2', name: 'Withholding Tax', rate: 1.0, taxType: 'Withholding', isDefault: false, isActive: true }
    ]);
  }
  createTaxConfig(t: Partial<TaxConfig>): Promise<void> { return this.post('tax-config', t); }
  updateTaxConfig(id: string, t: Partial<TaxConfig>): Promise<void> { return this.put(`tax-config/${id}`, t); }
  deleteTaxConfig(id: string): Promise<void> { return this.delete(`tax-config/${id}`); }

  // PaymentTerm
  getPaymentTerms(): Promise<PaymentTerm[]> {
    return this.getList<PaymentTerm>('payment-term').catch(() => [
      { id: 'pt-1', name: 'Net 30 Days', dueDays: 30, discountDays: 0, discountPercent: 0, description: 'Payment due within 30 days from invoice date' },
      { id: 'pt-2', name: '2/10 Net 30', dueDays: 30, discountDays: 10, discountPercent: 2.0, description: '2% discount if paid within 10 days, full due in 30 days' },
      { id: 'pt-3', name: 'Immediate / Cash on Delivery', dueDays: 0, discountDays: 0, discountPercent: 0, description: 'Payment required upon delivery' }
    ]);
  }
  createPaymentTerm(pt: Partial<PaymentTerm>): Promise<void> { return this.post('payment-term', pt); }
  updatePaymentTerm(id: string, pt: Partial<PaymentTerm>): Promise<void> { return this.put(`payment-term/${id}`, pt); }
  deletePaymentTerm(id: string): Promise<void> { return this.delete(`payment-term/${id}`); }
}
