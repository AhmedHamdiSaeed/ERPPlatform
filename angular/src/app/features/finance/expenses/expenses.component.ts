import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnterpriseApiService, ExpenseRequest } from '../../../core/services/api/enterprise-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './expenses.component.html'
})
export class ExpensesComponent {
  private enterpriseApi = inject(EnterpriseApiService);
  private toast = inject(ToastService);

  expenses = signal<ExpenseRequest[]>([]);
  showModal = signal(false);

  newExp: Partial<ExpenseRequest> = {
    employeeName: 'Ahmed Hamdi',
    category: 'Travel',
    amount: 150,
    description: '',
    date: new Date().toISOString().split('T')[0],
    status: 'Pending Approval'
  };

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.expenses.set(await this.enterpriseApi.getExpenses());
  }

  openAddModal() {
    this.newExp = {
      expenseCode: `EXP-2026-00${this.expenses().length + 1}`,
      employeeName: 'Ahmed Hamdi',
      category: 'Travel',
      amount: 150,
      description: '',
      date: new Date().toISOString().split('T')[0],
      status: 'Pending Approval'
    };
    this.showModal.set(true);
  }

  async saveExpense() {
    await this.enterpriseApi.createExpense(this.newExp);
    this.toast.success('Expense claim submitted for approval.');
    this.showModal.set(false);
    await this.loadData();
  }

  async approveExpense(id: string) {
    await this.enterpriseApi.approveExpense(id);
    this.toast.success('Expense approved & accounting journal entry generated!');
    await this.loadData();
  }
}
