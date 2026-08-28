import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface DeliveryNoteItem {
  id: string;
  deliveryNumber: string;
  salesOrderNumber: string;
  customerName: string;
  deliveryDate: string;
  dispatchWarehouse: string;
  carrier: string;
  status: 'Dispatched' | 'Delivered' | 'Returned';
}

@Component({
  selector: 'app-delivery-notes',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './delivery-notes.component.html'
})
export class DeliveryNotesComponent {
  private toast = inject(ToastService);

  deliveries = signal<DeliveryNoteItem[]>([
    { id: 'dn-1', deliveryNumber: 'DN-2026-001', salesOrderNumber: 'SO-2026-001', customerName: 'Acme Corporation', deliveryDate: '2026-08-12', dispatchWarehouse: 'Main Warehouse', carrier: 'Express Freight', status: 'Delivered' },
    { id: 'dn-2', deliveryNumber: 'DN-2026-002', salesOrderNumber: 'SO-2026-002', customerName: 'Global Tech Solutions', deliveryDate: '2026-08-19', dispatchWarehouse: 'Central Logistics Hub', carrier: 'DHL Logistics', status: 'Dispatched' }
  ]);

  markDelivered(id: string) {
    this.deliveries.update(list => list.map(d => d.id === id ? { ...d, status: 'Delivered' as const } : d));
    this.toast.success('Delivery Note marked as Successfully Delivered to Client.');
  }
}
