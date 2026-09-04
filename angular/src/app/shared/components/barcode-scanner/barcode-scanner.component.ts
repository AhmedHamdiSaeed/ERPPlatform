import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Product } from '../../../core/models/erp-models';
import { InventoryApiService } from '../../../core/services/api/inventory-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './barcode-scanner.component.html'
})
export class BarcodeScannerComponent {
  private inventoryApi = inject(InventoryApiService);
  private toast = inject(ToastService);

  isScanning = signal(false);
  scannedResult = signal<Product | null>(null);
  products = signal<Product[]>([]);
  loadingProducts = signal(false);
  loadError = signal<string | null>(null);
  manualCode = '';

  canScan() {
    return !this.loadingProducts() && !this.loadError() && this.products().length > 0;
  }

  constructor() {
    this.loadProducts();
  }

  async loadProducts() {
    this.loadingProducts.set(true);
    this.loadError.set(null);
    try {
      this.products.set(await this.inventoryApi.getProducts());
    } catch (e) {
      console.error('Failed to load products for scanner', e);
      this.loadError.set('Could not load product catalog for scanner.');
      this.toast.error('Could not load product catalog for scanner.');
    } finally {
      this.loadingProducts.set(false);
    }
  }

  startScanner() {
    if (this.products().length === 0) {
      this.toast.warning('No products available to scan.');
      return;
    }

    this.isScanning.set(true);
    this.scannedResult.set(null);

    setTimeout(() => {
      const sample = this.products()[0];
      this.scannedResult.set(sample);
      this.isScanning.set(false);
      this.toast.success(`Barcode Scanned: ${sample.sku} - ${sample.name}`);
    }, 1500);
  }

  lookupManual() {
    const input = this.manualCode.trim().toLowerCase();
    if (this.loadingProducts()) {
      this.toast.info('Product catalog is still loading. Please wait.');
      return;
    }
    if (this.products().length === 0) {
      this.toast.warning('No product catalog available for lookup.');
      return;
    }
    if (!input) {
      this.toast.warning('Please enter SKU or Barcode.');
      return;
    }

    const found = this.products().find(p =>
      p.sku.toLowerCase() === input || p.name.toLowerCase().includes(input)
    );

    if (found) {
      this.scannedResult.set(found);
      this.toast.success(`Item Found: ${found.name}`);
    } else {
      this.toast.error(`No inventory record matching "${this.manualCode}".`);
    }
  }
}
