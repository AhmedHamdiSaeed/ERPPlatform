import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnterpriseApiService, ManufacturingOrder } from '../../core/services/api/enterprise-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-manufacturing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manufacturing.component.html'
})
export class ManufacturingComponent {
  private enterpriseApi = inject(EnterpriseApiService);
  private toast = inject(ToastService);

  orders = signal<ManufacturingOrder[]>([]);
  showModal = signal(false);
  newMo: Partial<ManufacturingOrder> = { workCenter: 'Assembly Line A', status: 'In Production' };

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.orders.set(await this.enterpriseApi.getManufacturingOrders());
  }

  openAddModal() {
    this.newMo = {
      moNumber: `MO-2026-90${this.orders().length + 1}`,
      finishedProductName: 'Industrial Hydraulic Pump Motor 5HP',
      quantityToProduce: 100,
      workCenter: 'Assembly Line 1',
      scheduledStartDate: new Date().toISOString().split('T')[0],
      status: 'In Production'
    };
    this.showModal.set(true);
  }

  async saveOrder() {
    await this.enterpriseApi.createManufacturingOrder(this.newMo);
    this.toast.success('Manufacturing Order (MO) scheduled & raw materials reserved.');
    this.showModal.set(false);
    await this.loadData();
  }
}
