import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SalesApiService, SalesDashboardStats, SalesInvoice } from '../../../core/services/api/sales-api.service';

@Component({
  selector: 'app-sales-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sales-dashboard.component.html'
})
export class SalesDashboardComponent {
  private salesApi = inject(SalesApiService);

  stats = signal<SalesDashboardStats | null>(null);
  recentInvoices = signal<SalesInvoice[]>([]);

  constructor() {
    this.loadData();
  }

  async loadData() {
    const [s, inv] = await Promise.all([
      this.salesApi.getSalesStats(),
      this.salesApi.getInvoices()
    ]);
    this.stats.set(s);
    this.recentInvoices.set(inv.slice(0, 5));
  }
}
