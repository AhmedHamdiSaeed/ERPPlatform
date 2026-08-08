import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { PurchaseOrder } from '../../../core/models/erp-models';
import { MOCK_PURCHASE_ORDERS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchase-order-list.component.html'
})
export class PurchaseOrderListComponent {
  orders = signal<PurchaseOrder[]>(MOCK_PURCHASE_ORDERS);
  selectedPo = signal<PurchaseOrder | null>(null);

  createPo() {
    const newPo: PurchaseOrder = {
      id: `po-${Date.now()}`,
      poNumber: `PO-2026-880${Math.floor(3 + Math.random()*9)}`,
      supplierName: 'FurniCorp Ltd.',
      orderDate: new Date().toISOString().split('T')[0],
      deliveryDate: '2026-08-25',
      items: [
        { productName: 'Ergonomic Executive Mesh Chair', quantity: 15, unitPrice: 340, totalPrice: 5100 }
      ],
      subtotal: 5100,
      tax: 714,
      discount: 200,
      grandTotal: 5614,
      createdBy: 'Omar Farouk',
      status: 'Pending Approval'
    };
    this.orders.update(list => [newPo, ...list]);
  }
}
