import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MOCK_PRODUCTS } from '../../../core/mock/mock-data';
import { Product } from '../../../core/models/erp-models';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './barcode-scanner.component.html'
})
export class BarcodeScannerComponent {
  private toast = inject(ToastService);

  isScanning = signal(false);
  scannedResult = signal<Product | null>(null);
  manualCode = '';

  startScanner() {
    this.isScanning.set(true);
    this.scannedResult.set(null);

    // Simulate scanning after 1.5 seconds
    setTimeout(() => {
      const sample = MOCK_PRODUCTS[0];
      this.scannedResult.set(sample);
      this.isScanning.set(false);
      this.toast.success(`Barcode Scanned: ${sample.sku} - ${sample.name}`);
    }, 1500);
  }

  lookupManual() {
    if (!this.manualCode) {
      this.toast.warning('Please enter SKU or Barcode.');
      return;
    }

    const found = MOCK_PRODUCTS.find(p =>
      p.sku.toLowerCase() === this.manualCode.trim().toLowerCase() ||
      p.name.toLowerCase().includes(this.manualCode.trim().toLowerCase())
    );

    if (found) {
      this.scannedResult.set(found);
      this.toast.success(`Item Found: ${found.name}`);
    } else {
      this.toast.error(`No inventory record matching "${this.manualCode}".`);
    }
  }
}
