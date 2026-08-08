import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StockTransfer } from '../../../core/models/erp-models';
import { MOCK_STOCK_TRANSFERS, MOCK_PRODUCTS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './stock-transfer-list.component.html'
})
export class StockTransferListComponent {
  transfers = signal<StockTransfer[]>(MOCK_STOCK_TRANSFERS);
  products = MOCK_PRODUCTS;

  showModal = signal(false);

  newTrf: Partial<StockTransfer> = {
    sourceWarehouse: 'Main Warehouse',
    destinationWarehouse: 'Secondary Warehouse',
    productName: MOCK_PRODUCTS[0].name,
    quantity: 10
  };

  openTransferModal() {
    this.showModal.set(true);
  }

  submitTransfer() {
    const item: StockTransfer = {
      id: `st-${Date.now()}`,
      transferCode: `TRF-2026-00${Math.floor(10 + Math.random()*90)}`,
      sourceWarehouse: this.newTrf.sourceWarehouse!,
      destinationWarehouse: this.newTrf.destinationWarehouse!,
      productName: this.newTrf.productName!,
      quantity: this.newTrf.quantity || 10,
      requestedBy: 'Omar Farouk',
      date: new Date().toISOString().split('T')[0],
      status: 'Pending Approval'
    };
    this.transfers.update(list => [item, ...list]);
    this.showModal.set(false);
  }

  completeTransfer(id: string) {
    this.transfers.update(list => list.map(t => t.id === id ? { ...t, status: 'Completed' } as StockTransfer : t));
  }
}
