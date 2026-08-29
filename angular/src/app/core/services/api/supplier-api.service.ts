import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export interface SupplierProfile {
  id: string;
  supplierCode: string;
  companyName: string;
  email: string;
  phone: string;
  address: string;
  contactPerson: string;
  taxNumber: string;
  creditLimit: number;
  outstandingBalance: number;
  paymentTerms: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class SupplierApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  getSuppliers(): Promise<SupplierProfile[]> {
    return this.getList<SupplierProfile>('supplier');
  }

  createSupplier(s: Partial<SupplierProfile>): Promise<void> {
    return this.post('supplier', s);
  }

  deleteSupplier(id: string): Promise<void> {
    return this.delete(`supplier/${id}`);
  }
}
