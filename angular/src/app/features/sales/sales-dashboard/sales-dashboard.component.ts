import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SalesApiService, SalesDashboardStats, SalesInvoice } from '../../../core/services/api/sales-api.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

@Component({
  selector: 'app-sales-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe, AppDatePipe],
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
