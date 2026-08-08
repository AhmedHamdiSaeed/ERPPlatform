import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Warehouse } from '../../../core/models/erp-models';
import { MOCK_WAREHOUSES } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './warehouse-list.component.html'
})
export class WarehouseListComponent {
  warehouses = signal<Warehouse[]>(MOCK_WAREHOUSES);
}
