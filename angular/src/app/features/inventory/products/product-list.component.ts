import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product } from '../../../core/models/erp-models';
import { InventoryApiService } from '../../../core/services/api/inventory-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent {
  private inventoryApi = inject(InventoryApiService);
  private toast = inject(ToastService);

  products = signal<Product[]>([]);
  searchQuery = '';
  statusFilter = 'ALL';

  adjustProduct = signal<Product | null>(null);
  newStockVal = 0;

  constructor() {
    this.loadProducts();
  }

  async loadProducts() {
    try {
      this.products.set(await this.inventoryApi.getProducts());
    } catch (e) {
      console.error('Failed to load products', e);
      this.toast.error('Could not load products from the server.');
    }
  }

  filteredProducts() {
    return this.products().filter(p => {
      const matchQ = !this.searchQuery || p.name.toLowerCase().includes(this.searchQuery.toLowerCase()) || p.sku.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchS = this.statusFilter === 'ALL' || p.status === this.statusFilter;
      return matchQ && matchS;
    });
  }

  async openAddModal() {
    const sku = `PRD-NEW-00${Math.floor(Math.random()*90)}`;
    try {
      await this.inventoryApi.createProduct({
        sku,
        name: 'Wireless Ergonomic Mechanical Keyboard',
        category: 'Electronics',
        price: 180,
        stock: 25,
        reorderLevel: 5,
        unit: 'pcs',
        warehouseName: 'Main Warehouse',
        supplierName: 'TechSupply Co.'
      });
      await this.loadProducts();
      this.toast.success(`Product ${sku} added to catalog.`);
    } catch (e) {
      console.error('Failed to create product', e);
      this.toast.error('Failed to create the product.');
    }
  }

  openAdjustStock(p: Product) {
    this.adjustProduct.set(p);
    this.newStockVal = p.stock;
  }

  async saveStockAdjustment() {
    const target = this.adjustProduct();
    if (!target) return;
    try {
      await this.inventoryApi.adjustStock(target.id, this.newStockVal);
      await this.loadProducts();
      this.toast.success(`Stock adjusted for ${target.name}.`);
    } catch (e) {
      console.error('Failed to adjust stock', e);
      this.toast.error('Failed to adjust stock level.');
    }
    this.adjustProduct.set(null);
  }
}
