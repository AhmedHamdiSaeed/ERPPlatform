import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export interface PurchaseRequest {
  id: string;
  prNumber: string;
  departmentName: string;
  requestedBy: string;
  itemName: string;
  quantity: number;
  estimatedCost: number;
  status: 'Pending Approval' | 'Approved' | 'Rejected';
}

export interface RfqItem {
  id: string;
  rfqNumber: string;
  supplierName: string;
  title: string;
  issueDate: string;
  deadlineDate: string;
  status: 'Draft' | 'Sent' | 'Quotation Received' | 'Closed';
}

export interface GoodsReceiptItem {
  id: string;
  grnNumber: string;
  poNumber: string;
  supplierName: string;
  receivedDate: string;
  receivingWarehouse: string;
  qcStatus: 'Pending QC' | 'Passed' | 'Failed';
}

@Injectable({ providedIn: 'root' })
export class PurchaseApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  // Purchase Requests
  getPurchaseRequests(): Promise<PurchaseRequest[]> {
    return this.getList<PurchaseRequest>('purchase-request');
  }
  createPurchaseRequest(pr: Partial<PurchaseRequest>): Promise<void> { return this.post('purchase-request', pr); }
  approvePurchaseRequest(id: string): Promise<void> { return this.post(`purchase-request/${id}/approve`, {}); }

  // RFQ
  getRfqs(): Promise<RfqItem[]> {
    return this.getList<RfqItem>('rfq');
  }
  createRfq(rfq: Partial<RfqItem>): Promise<void> { return this.post('rfq', rfq); }

  // Goods Receipts & QC
  getGoodsReceipts(): Promise<GoodsReceiptItem[]> {
    return this.getList<GoodsReceiptItem>('goods-receipt');
  }
  createGoodsReceipt(grn: Partial<GoodsReceiptItem>): Promise<void> { return this.post('goods-receipt', grn); }
  passQualityCheck(id: string): Promise<void> { return this.post(`goods-receipt/${id}/pass-quality-check`, {}); }
}
