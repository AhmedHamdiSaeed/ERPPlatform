import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface CustomerProfile {
  id: string;
  customerCode: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  contactPerson: string;
  taxNumber: string;
  creditLimit: number;
  outstandingBalance: number;
  paymentTerms: string;
  currency: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CustomerApiService extends ErpApiService {
  getCustomers(): Promise<CustomerProfile[]> {
    return this.getList<CustomerProfile>('customer').catch(() => [
      { id: 'cust-1', customerCode: 'CUST-001', name: 'Acme Corporation', email: 'finance@acme.com', phone: '+20 2 3344 5566', address: 'Plot 12, 6th of October Industrial Zone', contactPerson: 'John Smith', taxNumber: 'EG-10928374', creditLimit: 100000, outstandingBalance: 16600, paymentTerms: 'Net 30 Days', currency: 'USD', isActive: true },
      { id: 'cust-2', customerCode: 'CUST-002', name: 'Global Tech Solutions', email: 'billing@globaltech.com', phone: '+20 2 2211 4455', address: 'Smart Village, Building C4', contactPerson: 'Sarah Jenkins', taxNumber: 'EG-98765432', creditLimit: 250000, outstandingBalance: 31920, paymentTerms: 'Net 45 Days', currency: 'USD', isActive: true },
      { id: 'cust-3', customerCode: 'CUST-003', name: 'Apex Industrial Co.', email: 'accounts@apex.com', phone: '+20 3 4455 6677', address: 'Alexandria Free Zone', contactPerson: 'Mahmoud Ali', taxNumber: 'EG-45678912', creditLimit: 50000, outstandingBalance: 9376, paymentTerms: 'Immediate / COD', currency: 'USD', isActive: true }
    ]);
  }

  createCustomer(c: Partial<CustomerProfile>): Promise<void> {
    return this.post('customer', c);
  }

  deleteCustomer(id: string): Promise<void> {
    return this.delete(`customer/${id}`);
  }
}
