import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Account, JournalEntry } from '../../core/models/erp-models';
import { SharedApiService } from '../../core/services/api/shared-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-finance-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './finance-dashboard.component.html'
})
export class FinanceDashboardComponent {
  private toast = inject(ToastService);
  private sharedApi = inject(SharedApiService);

  activeTab = signal<'coa' | 'journal' | 'pnl' | 'balanceSheet' | 'trialBalance' | 'aging'>('coa');

  accounts = signal<Account[]>([]);
  journalEntries = signal<JournalEntry[]>([]);

  showJournalModal = signal(false);
  newEntryDescription = '';
  newEntryDebit = 0;
  newEntryCredit = 0;

  constructor() {
    this.loadData();
  }

  async loadData() {
    try {
      const [accounts, entries] = await Promise.all([
        this.sharedApi.getAccounts(),
        this.sharedApi.getJournalEntries()
      ]);
      this.accounts.set(accounts);
      this.journalEntries.set(entries);
    } catch (e) {
      console.error('Failed to load finance data', e);
    }
  }

  totalAssets = computed(() =>
    this.accounts().filter(a => a.type === 'Asset').reduce((sum, a) => sum + a.balance, 0)
  );

  totalLiabilities = computed(() =>
    this.accounts().filter(a => a.type === 'Liability').reduce((sum, a) => sum + a.balance, 0)
  );

  totalRevenue = computed(() =>
    this.accounts().filter(a => a.type === 'Revenue').reduce((sum, a) => sum + a.balance, 0)
  );

  totalExpenses = computed(() =>
    this.accounts().filter(a => a.type === 'Expense').reduce((sum, a) => sum + a.balance, 0)
  );

  netProfit = computed(() => this.totalRevenue() - this.totalExpenses());

  openJournalModal() {
    this.newEntryDescription = '';
    this.newEntryDebit = 0;
    this.newEntryCredit = 0;
    this.showJournalModal.set(true);
  }

  async saveJournalEntry() {
    if (this.newEntryDebit !== this.newEntryCredit) {
      this.toast.error(`Double-entry mismatch! Debit ($${this.newEntryDebit}) must equal Credit ($${this.newEntryCredit}).`);
      return;
    }

    try {
      await this.sharedApi.createJournalEntry({
        entryNumber: `JE-2026-080${this.journalEntries().length + 1}`,
        description: this.newEntryDescription || 'General Ledger Adjustment',
        totalDebit: this.newEntryDebit,
        totalCredit: this.newEntryCredit,
        createdBy: 'Finance Admin'
      });
      await this.loadData();
      this.toast.success('Journal entry posted successfully to General Ledger.');
    } catch (e) {
      this.toast.error('Failed to post journal entry.');
    }
    this.showJournalModal.set(false);
  }
}
