import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

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
  getSuppliers(): Promise<SupplierProfile[]> {
    return this.getList<SupplierProfile>('supplier').catch(() => [
      { id: 'sup-1', supplierCode: 'SUP-001', companyName: 'TechSupply Co.', email: 'orders@techsupply.com', phone: '+1 800 555 0199', address: 'San Jose, CA, USA', contactPerson: 'Michael Brown', taxNumber: 'US-9918237', creditLimit: 200000, outstandingBalance: 18880, paymentTerms: 'Net 30 Days', isActive: true },
      { id: 'sup-2', supplierCode: 'SUP-002', companyName: 'FurniCorp Ltd.', email: 'sales@furnicorp.com', phone: '+44 20 7946 0912', address: 'London, UK', contactPerson: 'Emma Watson', taxNumber: 'GB-12345678', creditLimit: 150000, outstandingBalance: 4200, paymentTerms: 'Net 15 Days', isActive: true },
      { id: 'sup-3', supplierCode: 'SUP-003', companyName: 'HeavyMachinery Inc.', email: 'info@heavymachinery.de', phone: '+49 30 123456', address: 'Berlin, Germany', contactPerson: 'Hans Gruber', taxNumber: 'DE-87654321', creditLimit: 500000, outstandingBalance: 0, paymentTerms: 'Net 60 Days', isActive: true }
    ]);
  }

  createSupplier(s: Partial<SupplierProfile>): Promise<void> {
    return this.post('supplier', s);
  }

  deleteSupplier(id: string): Promise<void> {
    return this.delete(`supplier/${id}`);
  }
}
