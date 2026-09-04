import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { Warehouse } from '../../../core/models/erp-models';
import { InventoryApiService } from '../../../core/services/api/inventory-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslatePipe],
  templateUrl: './warehouse-list.component.html'
})
export class WarehouseListComponent {
  private inventoryApi = inject(InventoryApiService);
  private toast = inject(ToastService);

  warehouses = signal<Warehouse[]>([]);

  constructor() {
    this.loadWarehouses();
  }

  async loadWarehouses() {
    try {
      this.warehouses.set(await this.inventoryApi.getWarehouses());
    } catch (e) {
      console.error('Failed to load warehouses', e);
      this.toast.error('Could not load warehouses from the server.');
    }
  }
}
