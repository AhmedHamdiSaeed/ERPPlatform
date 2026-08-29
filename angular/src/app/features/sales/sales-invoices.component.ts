import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StateService } from '../../core/services/state.service';
import { PERMISSIONS } from '../../core/models/permissions';

export interface SalesInvoice {
  id: string;
  invoiceNumber: string;
  customerName: string;
  issueDate: string;
  dueDate: string;
  amount: number;
  tax: number;
  totalAmount: number;
  status: 'Paid' | 'Pending' | 'Overdue' | 'Draft';
}

@Component({
  selector: 'app-sales-invoices',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './sales-invoices.component.html'
})
export class SalesInvoicesComponent {
  state = inject(StateService);
  readonly PERMISSIONS = PERMISSIONS;
  invoices = signal<SalesInvoice[]>([
    {
      id: 'inv-1',
      invoiceNumber: 'INV-2026-001',
      customerName: 'Acme International Corp',
      issueDate: '2026-08-01',
      dueDate: '2026-08-15',
      amount: 14500,
      tax: 2030,
      totalAmount: 16530,
      status: 'Paid'
    },
    {
      id: 'inv-2',
      invoiceNumber: 'INV-2026-002',
      customerName: 'Global Logistics Hub Ltd',
      issueDate: '2026-08-10',
      dueDate: '2026-08-24',
      amount: 8900,
      tax: 1246,
      totalAmount: 10146,
      status: 'Pending'
    },
    {
      id: 'inv-3',
      invoiceNumber: 'INV-2026-003',
      customerName: 'Delta Engineering Solutions',
      issueDate: '2026-07-15',
      dueDate: '2026-07-29',
      amount: 22000,
      tax: 3080,
      totalAmount: 25080,
      status: 'Overdue'
    }
  ]);

  searchQuery = '';
  statusFilter = 'ALL';
  showCreateModal = signal(false);

  newInvoice: Partial<SalesInvoice> = {
    customerName: '',
    amount: 5000,
    dueDate: '2026-09-01'
  };

  filteredInvoices() {
    return this.invoices().filter(inv => {
      const matchQ = !this.searchQuery || inv.customerName.toLowerCase().includes(this.searchQuery.toLowerCase()) || inv.invoiceNumber.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchS = this.statusFilter === 'ALL' || inv.status === this.statusFilter;
      return matchQ && matchS;
    });
  }

  totalRevenue() {
    return this.invoices().filter(i => i.status === 'Paid').reduce((sum, i) => sum + i.totalAmount, 0);
  }

  pendingRevenue() {
    return this.invoices().filter(i => i.status === 'Pending').reduce((sum, i) => sum + i.totalAmount, 0);
  }

  createInvoice() {
    const amt = this.newInvoice.amount || 0;
    const tax = amt * 0.14;
    const item: SalesInvoice = {
      id: `inv-${Date.now()}`,
      invoiceNumber: `INV-2026-00${Math.floor(4 + Math.random() * 90)}`,
      customerName: this.newInvoice.customerName || 'New Client Ltd',
      issueDate: new Date().toISOString().split('T')[0],
      dueDate: this.newInvoice.dueDate || '2026-09-15',
      amount: amt,
      tax: tax,
      totalAmount: amt + tax,
      status: 'Pending'
    };
    this.invoices.update(list => [item, ...list]);
    this.showCreateModal.set(false);
  }

  markAsPaid(id: string) {
    this.invoices.update(list => list.map(i => i.id === id ? { ...i, status: 'Paid' as const } : i));
  }
}
