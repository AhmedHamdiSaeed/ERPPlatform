import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

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
  // Purchase Requests
  getPurchaseRequests(): Promise<PurchaseRequest[]> {
    return this.getList<PurchaseRequest>('purchase-request').catch(() => [
      { id: 'pr-1', prNumber: 'PR-2026-001', departmentName: 'Information Technology', requestedBy: 'Ahmed Hamdi', itemName: 'High-Performance Workstations (10 units)', quantity: 10, estimatedCost: 18000, status: 'Approved' },
      { id: 'pr-2', prNumber: 'PR-2026-002', departmentName: 'Supply Chain & Logistics', requestedBy: 'Omar Farouk', itemName: 'Forklift Hydraulic Spare Parts', quantity: 4, estimatedCost: 3500, status: 'Pending Approval' }
    ]);
  }
  createPurchaseRequest(pr: Partial<PurchaseRequest>): Promise<void> { return this.post('purchase-request', pr); }
  approvePurchaseRequest(id: string): Promise<void> { return this.post(`purchase-request/${id}/approve`, {}); }

  // RFQ
  getRfqs(): Promise<RfqItem[]> {
    return this.getList<RfqItem>('rfq').catch(() => [
      { id: 'rfq-1', rfqNumber: 'RFQ-2026-801', supplierName: 'TechSupply Co.', title: 'Request for Quotation - 48-Port Switches', issueDate: '2026-08-15', deadlineDate: '2026-08-25', status: 'Sent' },
      { id: 'rfq-2', rfqNumber: 'RFQ-2026-802', supplierName: 'FurniCorp Ltd.', title: 'RFQ - Executive Office Desks', issueDate: '2026-08-18', deadlineDate: '2026-08-28', status: 'Quotation Received' }
    ]);
  }
  createRfq(rfq: Partial<RfqItem>): Promise<void> { return this.post('rfq', rfq); }

  // Goods Receipts & QC
  getGoodsReceipts(): Promise<GoodsReceiptItem[]> {
    return this.getList<GoodsReceiptItem>('goods-receipt').catch(() => [
      { id: 'grn-1', grnNumber: 'GRN-2026-0501', poNumber: 'PO-2026-8801', supplierName: 'TechSupply Co.', receivedDate: '2026-08-20', receivingWarehouse: 'Main Warehouse', qcStatus: 'Passed' },
      { id: 'grn-2', grnNumber: 'GRN-2026-0502', poNumber: 'PO-2026-8802', supplierName: 'FurniCorp Ltd.', receivedDate: '2026-08-22', receivingWarehouse: 'Secondary Warehouse', qcStatus: 'Pending QC' }
    ]);
  }
  createGoodsReceipt(grn: Partial<GoodsReceiptItem>): Promise<void> { return this.post('goods-receipt', grn); }
  passQualityCheck(id: string): Promise<void> { return this.post(`goods-receipt/${id}/pass-qc`, {}); }
}
