import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StockTransfer, Product } from '../../../core/models/erp-models';
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
  products = signal<Product[]>([]);
  loading = signal(false);
  submitting = signal(false);
  loadError = signal<string | null>(null);

  showModal = signal(false);

  newTrf: Partial<StockTransfer> = {
    sourceWarehouse: 'Main Warehouse',
    destinationWarehouse: 'Secondary Warehouse',
    productName: '',
    quantity: 10
  };

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.loading.set(true);
    this.loadError.set(null);
    try {
      const [transfers, products] = await Promise.all([
        this.inventoryApi.getStockTransfers(),
        this.inventoryApi.getProducts()
      ]);
      this.transfers.set(transfers);
      this.products.set(products);
      if (!this.newTrf.productName && products.length > 0) {
        this.newTrf.productName = products[0].name;
      }
    } catch (e) {
      console.error('Failed to load stock transfer data', e);
      this.loadError.set('Could not load stock transfer data from the server.');
      this.toast.error('Could not load stock transfer data from the server.');
    } finally {
      this.loading.set(false);
    }
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
    if (this.products().length === 0) {
      this.toast.warning('No products available for transfer.');
      return;
    }
    if (!this.newTrf.productName) {
      this.newTrf.productName = this.products()[0].name;
    }
    this.showModal.set(true);
  }

  async submitTransfer() {
    if (!this.newTrf.productName || !this.newTrf.sourceWarehouse || !this.newTrf.destinationWarehouse) {
      this.toast.warning('Please complete all transfer fields.');
      return;
    }
    if ((this.newTrf.quantity || 0) <= 0) {
      this.toast.warning('Transfer quantity must be greater than zero.');
      return;
    }

    this.submitting.set(true);
    try {
      await this.inventoryApi.createStockTransfer({
        transferCode: `TRF-2026-00${Math.floor(10 + Math.random() * 90)}`,
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
      this.showModal.set(false);
    } catch (e) {
      console.error('Failed to create stock transfer', e);
      this.toast.error('Failed to submit the stock transfer.');
    } finally {
      this.submitting.set(false);
    }
  }

  async completeTransfer(id: string) {
    if (this.submitting()) return;
    this.submitting.set(true);
    try {
      await this.inventoryApi.updateStockTransferStatus(id, 'Completed');
      await this.loadTransfers();
      this.toast.success('Transfer marked as completed.');
    } catch (e) {
      console.error('Failed to update stock transfer', e);
      this.toast.error('Failed to update the stock transfer status.');
    } finally {
      this.submitting.set(false);
    }
  }
}
