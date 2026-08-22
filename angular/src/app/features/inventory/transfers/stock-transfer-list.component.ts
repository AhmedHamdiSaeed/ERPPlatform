import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StockTransfer, Product } from '../../../core/models/erp-models';
import { MOCK_PRODUCTS } from '../../../core/mock/mock-data';
import { InventoryApiService } from '../../../core/services/api/inventory-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './stock-transfer-list.component.html'
})
export class StockTransferListComponent {
  private inventoryApi = inject(InventoryApiService);
  private toast = inject(ToastService);

  transfers = signal<StockTransfer[]>([]);
  products: Product[] = MOCK_PRODUCTS;

  showModal = signal(false);

  newTrf: Partial<StockTransfer> = {
    sourceWarehouse: 'Main Warehouse',
    destinationWarehouse: 'Secondary Warehouse',
    productName: MOCK_PRODUCTS[0].name,
    quantity: 10
  };

  constructor() {
    this.loadTransfers();
  }

  async loadTransfers() {
    try {
      this.transfers.set(await this.inventoryApi.getStockTransfers());
    } catch (e) {
      console.error('Failed to load stock transfers', e);
      this.toast.error('Could not load stock transfers from the server.');
    }
  }

  openTransferModal() {
    this.showModal.set(true);
  }

  async submitTransfer() {
    try {
      await this.inventoryApi.createStockTransfer({
        transferCode: `TRF-2026-00${Math.floor(10 + Math.random()*90)}`,
        sourceWarehouse: this.newTrf.sourceWarehouse!,
        destinationWarehouse: this.newTrf.destinationWarehouse!,
        productName: this.newTrf.productName!,
        quantity: this.newTrf.quantity || 10,
        requestedBy: 'Omar Farouk',
        date: new Date().toISOString(),
        status: 'Pending Approval'
      });
      await this.loadTransfers();
      this.toast.success('Stock transfer submitted for approval.');
    } catch (e) {
      console.error('Failed to create stock transfer', e);
      this.toast.error('Failed to submit the stock transfer.');
    }
    this.showModal.set(false);
  }

  async completeTransfer(id: string) {
    try {
      await this.inventoryApi.updateStockTransferStatus(id, 'Completed');
      await this.loadTransfers();
      this.toast.success('Transfer marked as completed.');
    } catch (e) {
      console.error('Failed to update stock transfer', e);
      this.toast.error('Failed to update the stock transfer status.');
    }
  }
}
