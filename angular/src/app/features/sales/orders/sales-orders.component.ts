import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface SalesOrder {
  id: string;
  orderNumber: string;
  customerName: string;
  orderDate: string;
  expectedDeliveryDate: string;
  totalAmount: number;
  status: 'Draft' | 'Approved' | 'Stock Reserved' | 'Delivered' | 'Invoiced';
}

@Component({
  selector: 'app-sales-orders',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './sales-orders.component.html'
})
export class SalesOrdersComponent {
  private toast = inject(ToastService);

  orders = signal<SalesOrder[]>([
    { id: 'so-1', orderNumber: 'SO-2026-001', customerName: 'Acme Corporation', orderDate: '2026-08-10', expectedDeliveryDate: '2026-08-17', totalAmount: 16600, status: 'Stock Reserved' },
    { id: 'so-2', orderNumber: 'SO-2026-002', customerName: 'Global Tech Solutions', orderDate: '2026-08-18', expectedDeliveryDate: '2026-08-25', totalAmount: 31920, status: 'Approved' }
  ]);

  showModal = signal(false);
  newSo: Partial<SalesOrder> = {};

  openAddModal() {
    this.newSo = {
      orderNumber: `SO-2026-00${this.orders().length + 1}`,
      customerName: 'Delta Systems',
      orderDate: new Date().toISOString().split('T')[0],
      expectedDeliveryDate: new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0],
      totalAmount: 12500,
      status: 'Approved'
    };
    this.showModal.set(true);
  }

  saveOrder() {
    this.orders.update(list => [{ ...this.newSo, id: `so-${Date.now()}` } as SalesOrder, ...list]);
    this.toast.success('Sales order created and inventory stock reserved.');
    this.showModal.set(false);
  }

  reserveStock(id: string) {
    this.orders.update(list => list.map(o => o.id === id ? { ...o, status: 'Stock Reserved' as const } : o));
    this.toast.success('Inventory stock successfully reserved for order.');
  }
}
