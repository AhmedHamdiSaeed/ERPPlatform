import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

export interface StockOperationLog {
  id: string;
  operationType: 'Stock In' | 'Stock Out' | 'Stock Adjustment' | 'Damaged Write-Off' | 'Expired Write-Off';
  productName: string;
  sku: string;
  warehouseName: string;
  quantityChange: number;
  date: string;
  performedBy: string;
  reason: string;
}

@Component({
  selector: 'app-stock-operations',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './stock-operations.component.html'
})
export class StockOperationsComponent {
  private toast = inject(ToastService);

  logs = signal<StockOperationLog[]>([
    { id: 'op-1', operationType: 'Stock In', productName: 'Dell UltraSharp 27" 4K Monitor', sku: 'PRD-ELC-001', warehouseName: 'Main Warehouse', quantityChange: +15, date: '2026-08-22', performedBy: 'Omar Farouk', reason: 'Received shipment GRN-2026-0501' },
    { id: 'op-2', operationType: 'Stock Out', productName: 'Logitech MX Master 3S Mouse', sku: 'PRD-ELC-002', warehouseName: 'Main Warehouse', quantityChange: -5, date: '2026-08-24', performedBy: 'Omar Farouk', reason: 'Dispatched for Sales Order SO-2026-001' },
    { id: 'op-3', operationType: 'Damaged Write-Off', productName: 'Ergonomic Executive Mesh Chair', sku: 'PRD-OFC-101', warehouseName: 'Secondary Warehouse', quantityChange: -2, date: '2026-08-26', performedBy: 'Magdy Zaky', reason: 'Damaged during forklift transport' }
  ]);

  showModal = signal(false);
  newOp: Partial<StockOperationLog> = { operationType: 'Stock In', warehouseName: 'Main Warehouse', quantityChange: 1 };

  openAddModal() {
    this.newOp = { operationType: 'Stock In', productName: 'Dell UltraSharp 27" 4K Monitor', sku: 'PRD-ELC-001', warehouseName: 'Main Warehouse', quantityChange: 10, reason: 'Physical Stock Count Audit' };
    this.showModal.set(true);
  }

  saveOperation() {
    const qty = this.newOp.operationType === 'Stock In' ? Math.abs(this.newOp.quantityChange || 1) : -Math.abs(this.newOp.quantityChange || 1);
    const entry: StockOperationLog = {
      id: `op-${Date.now()}`,
      operationType: this.newOp.operationType || 'Stock In',
      productName: this.newOp.productName || 'Sample Item',
      sku: this.newOp.sku || 'PRD-001',
      warehouseName: this.newOp.warehouseName || 'Main Warehouse',
      quantityChange: qty,
      date: new Date().toISOString().split('T')[0],
      performedBy: 'Omar Farouk',
      reason: this.newOp.reason || 'Manual Adjustment'
    };
    this.logs.update(list => [entry, ...list]);
    this.toast.success('Stock operation logged and warehouse inventory level updated.');
    this.showModal.set(false);
  }
}
