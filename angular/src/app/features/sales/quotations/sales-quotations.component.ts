import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SalesApiService, SalesQuotation } from '../../../core/services/api/sales-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-sales-quotations',
  standalone: true,
  imports: [FormsModule, RouterModule, CommonModule],
  templateUrl: './sales-quotations.component.html'
})
export class SalesQuotationsComponent {
  private salesApi = inject(SalesApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  quotations = signal<SalesQuotation[]>([]);
  statusFilter = signal<string>('ALL');
  showModal = signal(false);

  newQt: Partial<SalesQuotation> = {
    customerName: '',
    customerEmail: '',
    subtotal: 0,
    taxAmount: 0,
    discount: 0,
    totalAmount: 0,
    status: 'Draft',
    createdBy: 'Ahmed Hamdi'
  };

  constructor() {
    this.loadQuotations();
  }

  async loadQuotations() {
    this.quotations.set(await this.salesApi.getQuotations());
  }

  filteredQuotations = computed(() => {
    const list = this.quotations();
    const filter = this.statusFilter();
    return filter === 'ALL' ? list : list.filter(q => q.status === filter);
  });

  openAddModal() {
    this.newQt = {
      quotationNumber: `QT-2026-${Math.floor(Math.random() * 900 + 100)}`,
      customerName: '',
      customerEmail: '',
      subtotal: 5000,
      taxAmount: 700,
      discount: 200,
      totalAmount: 5500,
      status: 'Sent',
      createdBy: 'Ahmed Hamdi'
    };
    this.showModal.set(true);
  }

  async saveQuotation() {
    await this.salesApi.createQuotation(this.newQt);
    this.toast.success('Sales quotation generated.');
    this.showModal.set(false);
    await this.loadQuotations();
  }

  async convertToInvoice(id: string) {
    const confirmed = await this.dialog.confirm({
      title: 'Convert Quotation to Invoice',
      message: 'Are you sure you want to convert this quotation into a formal sales invoice?',
      confirmText: 'Convert Now',
      cancelText: 'Cancel'
    });

    if (confirmed) {
      await this.salesApi.convertToInvoice(id);
      this.toast.success('Quotation converted to Sales Invoice!');
      await this.loadQuotations();
    }
  }
}
