import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { PurchaseOrder } from '../../../core/models/erp-models';
import { InventoryApiService } from '../../../core/services/api/inventory-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './purchase-order-list.component.html'
})
export class PurchaseOrderListComponent {
  private inventoryApi = inject(InventoryApiService);
  private toast = inject(ToastService);

  orders = signal<PurchaseOrder[]>([]);
  selectedPo = signal<PurchaseOrder | null>(null);

  constructor() {
    this.loadOrders();
  }

  async loadOrders() {
    try {
      this.orders.set(await this.inventoryApi.getPurchaseOrders());
    } catch (e) {
      console.error('Failed to load purchase orders', e);
      this.toast.error('Could not load purchase orders from the server.');
    }
  }

  async createPo() {
    const items = [
      { productName: 'Ergonomic Executive Mesh Chair', quantity: 15, unitPrice: 340, totalPrice: 5100 }
    ];
    const subtotal = 5100;
    try {
      await this.inventoryApi.createPurchaseOrder({
        poNumber: `PO-2026-880${Math.floor(3 + Math.random()*9)}`,
        supplierName: 'FurniCorp Ltd.',
        orderDate: new Date().toISOString().split('T')[0],
        deliveryDate: '2026-08-25',
        items,
        subtotal,
        tax: Math.round(subtotal * 0.14 * 100) / 100,
        discount: 200,
        grandTotal: subtotal + Math.round(subtotal * 0.14) - 200,
        createdBy: 'Omar Farouk',
        status: 'Pending Approval'
      });
      await this.loadOrders();
      this.toast.success('Purchase order created.');
    } catch (e) {
      console.error('Failed to create purchase order', e);
      this.toast.error('Failed to create the purchase order.');
    }
  }

  async approve(order: PurchaseOrder) {
    try {
      await this.inventoryApi.updatePurchaseOrderStatus(order.id, 'Approved');
      await this.loadOrders();
      this.toast.success(`Purchase order ${order.poNumber} approved.`);
    } catch (e) {
      console.error('Failed to approve purchase order', e);
      this.toast.error('Failed to approve the purchase order.');
    }
  }
}
