import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Product } from '../../../core/models/erp-models';
import { MOCK_PRODUCTS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent {
  products = signal<Product[]>(MOCK_PRODUCTS);
  searchQuery = '';
  statusFilter = 'ALL';

  adjustProduct = signal<Product | null>(null);
  newStockVal = 0;

  filteredProducts() {
    return this.products().filter(p => {
      const matchQ = !this.searchQuery || p.name.toLowerCase().includes(this.searchQuery.toLowerCase()) || p.sku.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchS = this.statusFilter === 'ALL' || p.status === this.statusFilter;
      return matchQ && matchS;
    });
  }

  openAddModal() {
    const newP: Product = {
      id: `prod-${Date.now()}`,
      sku: `PRD-NEW-00${Math.floor(Math.random()*90)}`,
      name: 'Wireless Ergonomic Mechanical Keyboard',
      category: 'Electronics',
      price: 180,
      stock: 25,
      reorderLevel: 5,
      unit: 'pcs',
      warehouseName: 'Main Warehouse',
      status: 'In Stock',
      supplierName: 'TechSupply Co.'
    };
    this.products.update(list => [newP, ...list]);
  }

  openAdjustStock(p: Product) {
    this.adjustProduct.set(p);
    this.newStockVal = p.stock;
  }

  saveStockAdjustment() {
    const target = this.adjustProduct();
    if (!target) return;
    const nextStatus = this.newStockVal === 0 ? 'Out of Stock' : (this.newStockVal <= target.reorderLevel ? 'Low Stock' : 'In Stock');
    this.products.update(list => list.map(p => p.id === target.id ? { ...p, stock: this.newStockVal, status: nextStatus } : p));
    this.adjustProduct.set(null);
  }
}
