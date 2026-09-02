import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

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
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/inventory`;
  }

  getInvoices(): Promise<SalesInvoice[]> {
    return this.getList<SalesInvoice>('sales-invoice');
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
    return this.getList<SalesQuotation>('sales-quotation');
  }

  createQuotation(qt: Partial<SalesQuotation>): Promise<void> {
    return this.post('sales-quotation', qt);
  }

  convertToInvoice(quotationId: string): Promise<void> {
    // POST /api/inventory/sales-quotation/convert-to-invoice/{quotationId}
    return this.post(`sales-quotation/convert-to-invoice/${encodeURIComponent(quotationId)}`, {});
  }

  getSalesStats(): Promise<SalesDashboardStats> {
    return this.get<SalesDashboardStats>('sales-invoice/stats');
  }
}
