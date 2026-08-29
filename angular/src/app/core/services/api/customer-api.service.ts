import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

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
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  getCustomers(): Promise<CustomerProfile[]> {
    return this.getList<CustomerProfile>('customer');
  }

  createCustomer(c: Partial<CustomerProfile>): Promise<void> {
    return this.post('customer', c);
  }

  deleteCustomer(id: string): Promise<void> {
    return this.delete(`customer/${id}`);
  }
}
