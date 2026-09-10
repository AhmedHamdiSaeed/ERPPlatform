import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import {
  PayrollApiService,
  SalaryComponentDto
} from '../../../core/services/api/payroll-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-salary-components',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './salary-components.component.html'
})
export class SalaryComponentsComponent {
  private api = inject(PayrollApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  readonly types = ['Earning', 'Deduction', 'EmployerContribution'];
  readonly calcTypes = ['Fixed', 'Percentage', 'Formula'];

  components = signal<SalaryComponentDto[]>([]);
  showModal = signal(false);
  saving = signal(false);
  editingId: string | null = null;

  draft: Partial<SalaryComponentDto> = this.emptyDraft();

  constructor() {
    this.load();
  }

  private emptyDraft(): Partial<SalaryComponentDto> {
    return {
      code: '',
      name: '',
      type: 'Earning',
      calculationType: 'Fixed',
      amount: 0,
      percentageOfComponentCode: 'BASIC',
      formula: '',
      isTaxable: true,
      isRecurring: true,
      countsAsEmployerCost: false,
      isActive: true,
      effectiveFrom: new Date().toISOString().split('T')[0],
      effectiveTo: null,
      glAccount: '',
      costCenter: '',
      sortOrder: 0
    };
  }

  async load(): Promise<void> {
    try {
      const list = await this.api.getSalaryComponents();
      this.components.set([...list].sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name)));
    } catch {
      this.toast.error('Could not load salary components.');
    }
  }

  openAdd(): void {
    this.editingId = null;
    this.draft = this.emptyDraft();
    this.showModal.set(true);
  }

  openEdit(c: SalaryComponentDto): void {
    this.editingId = c.id;
    this.draft = { ...c, effectiveFrom: (c.effectiveFrom || '').split('T')[0], effectiveTo: c.effectiveTo ? (c.effectiveTo as string).split('T')[0] : null };
    this.showModal.set(true);
  }

  async save(): Promise<void> {
    if (!this.draft.code?.trim() || !this.draft.name?.trim()) {
      this.toast.warning('A code and a name are required.');
      return;
    }
    this.saving.set(true);
    try {
      if (this.editingId) {
        await this.api.updateSalaryComponent(this.editingId, this.draft);
        this.toast.success('Salary component updated.');
      } else {
        await this.api.createSalaryComponent(this.draft);
        this.toast.success('Salary component created.');
      }
      this.showModal.set(false);
      await this.load();
    } catch {
      this.toast.error('Failed to save the salary component.');
    } finally {
      this.saving.set(false);
    }
  }

  async remove(c: SalaryComponentDto): Promise<void> {
    const ok = await this.dialog.confirm({
      title: 'Delete salary component',
      message: `Delete "${c.name}" (${c.code})? Future payroll runs will no longer apply it.`,
      confirmText: 'Delete',
      type: 'danger'
    });
    if (!ok) return;
    try {
      await this.api.deleteSalaryComponent(c.id);
      this.toast.success('Salary component deleted.');
      await this.load();
    } catch {
      this.toast.error('Failed to delete the salary component.');
    }
  }

  typeClass(type: string): string {
    switch (type) {
      case 'Deduction':
        return 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300';
      case 'EmployerContribution':
        return 'bg-slate-100 text-slate-600 dark:bg-slate-700 dark:text-slate-300';
      default:
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';
    }
  }

  calcLabel(c: SalaryComponentDto): string {
    if (c.calculationType === 'Percentage') return `${c.amount}% of ${c.percentageOfComponentCode}`;
    if (c.calculationType === 'Formula') return c.formula ? `f(${c.formula})` : 'Formula';
    return this.money(c.amount);
  }

  money(value: number): string {
    return (value ?? 0).toLocaleString(undefined, { maximumFractionDigits: 0 });
  }
}
