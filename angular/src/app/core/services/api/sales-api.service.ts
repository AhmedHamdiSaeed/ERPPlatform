import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface SalesInvoice {
  id: string;
  invoiceNumber: string;
  customerName: string;
  customerEmail: string;
  issueDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  discount: number;
  totalAmount: number;
  status: 'Draft' | 'Sent' | 'Paid' | 'Overdue' | 'Cancelled';
  notes?: string;
  createdBy: string;
}

export interface SalesQuotation {
  id: string;
  quotationNumber: string;
  customerName: string;
  customerEmail: string;
  issueDate: string;
  expiryDate: string;
  subtotal: number;
  taxAmount: number;
  discount: number;
  totalAmount: number;
  status: 'Draft' | 'Sent' | 'Accepted' | 'Rejected' | 'Expired';
  notes?: string;
  createdBy: string;
}

export interface SalesDashboardStats {
  totalInvoices: number;
  paidInvoices: number;
  pendingAmount: number;
  paidAmount: number;
}

@Injectable({ providedIn: 'root' })
export class SalesApiService extends ErpApiService {
  getInvoices(): Promise<SalesInvoice[]> {
    return this.getList<SalesInvoice>('sales-invoice').catch(() => [
      { id: 'inv-001', invoiceNumber: 'INV-2026-001', customerName: 'Acme Corporation', customerEmail: 'finance@acme.com', issueDate: '2026-08-01', dueDate: '2026-08-31', subtotal: 15000, taxAmount: 2100, discount: 500, totalAmount: 16600, status: 'Paid', createdBy: 'Ahmed Hamdi' },
      { id: 'inv-002', invoiceNumber: 'INV-2026-002', customerName: 'Global Tech Solutions', customerEmail: 'billing@globaltech.com', issueDate: '2026-08-10', dueDate: '2026-09-10', subtotal: 28000, taxAmount: 3920, discount: 0, totalAmount: 31920, status: 'Sent', createdBy: 'Nour El-Din' },
      { id: 'inv-003', invoiceNumber: 'INV-2026-003', customerName: 'Apex Industrial Co.', customerEmail: 'accounts@apex.com', issueDate: '2026-08-15', dueDate: '2026-09-15', subtotal: 8400, taxAmount: 1176, discount: 200, totalAmount: 9376, status: 'Draft', createdBy: 'Ahmed Hamdi' }
    ]);
  }

  createInvoice(inv: Partial<SalesInvoice>): Promise<void> {
    return this.post('sales-invoice', inv);
  }

  markAsPaid(id: string): Promise<void> {
    return this.post(`sales-invoice/${id}/mark-as-paid`, {});
  }

  deleteInvoice(id: string): Promise<void> {
    return this.delete(`sales-invoice/${id}`);
  }

  getQuotations(): Promise<SalesQuotation[]> {
    return this.getList<SalesQuotation>('sales-quotation').catch(() => [
      { id: 'qt-001', quotationNumber: 'QT-2026-101', customerName: 'Horizon Traders', customerEmail: 'info@horizontraders.com', issueDate: '2026-08-05', expiryDate: '2026-08-25', subtotal: 12000, taxAmount: 1680, discount: 0, totalAmount: 13680, status: 'Sent', createdBy: 'Nour El-Din' },
      { id: 'qt-002', quotationNumber: 'QT-2026-102', customerName: 'Delta Systems', customerEmail: 'orders@deltasystems.io', issueDate: '2026-08-12', expiryDate: '2026-08-28', subtotal: 45000, taxAmount: 6300, discount: 1500, totalAmount: 49800, status: 'Draft', createdBy: 'Ahmed Hamdi' }
    ]);
  }

  createQuotation(qt: Partial<SalesQuotation>): Promise<void> {
    return this.post('sales-quotation', qt);
  }

  convertToInvoice(quotationId: string): Promise<void> {
    return this.post(`sales-quotation/${quotationId}/convert-to-invoice`, {});
  }

  getSalesStats(): Promise<SalesDashboardStats> {
    return this.get<SalesDashboardStats>('sales-invoice/stats').catch(() => ({
      totalInvoices: 18,
      paidInvoices: 14,
      pendingAmount: 41296,
      paidAmount: 185400
    }));
  }
}
