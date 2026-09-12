import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import {
  PayrollApiService,
  PayrollRunDto,
  PayslipDto,
  PayrollPreviewDto,
  PayrollAnomalyDto
} from '../../../core/services/api/payroll-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-payroll-management',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './payroll-management.component.html'
})
export class PayrollManagementComponent {
  private api = inject(PayrollApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  readonly LOCKED_STATUSES = ['Finalized', 'Posted'];

  // ── Runs ──
  runs = signal<PayrollRunDto[]>([]);

  // ── Simulation / preview ──
  simPeriod = signal(this.api.defaultPeriod);
  preview = signal<PayrollPreviewDto | null>(null);
  simulating = signal(false);

  // ── Payslips ──
  payslipPeriod = signal(this.api.defaultPeriod);
  payslips = signal<PayslipDto[]>([]);
  showPayslipModal = signal(false);
  currentPayslip: PayslipDto | null = null;

  // ── Process run modal ──
  showProcessModal = signal(false);
  processPeriod = signal(this.api.defaultPeriod);
  processing = signal(false);

  constructor() {
    this.loadRuns();
    this.loadPayslips();
    this.simulate();
  }

  // ── Loaders ──
  async loadRuns(): Promise<void> {
    try {
      this.runs.set(await this.api.getPayrollRuns());
    } catch {
      this.toast.error('Could not load payroll runs.');
    }
  }

  async loadPayslips(): Promise<void> {
    try {
      this.payslips.set(await this.api.getPayslips(this.payslipPeriod()));
    } catch {
      this.toast.error('Could not load payslips.');
    }
  }

  async simulate(): Promise<void> {
    this.simulating.set(true);
    try {
      const data = await this.api.getPreview(this.simPeriod());
      this.preview.set(data);
    } catch {
      this.toast.error('Could not compute the simulation.');
      this.preview.set(null);
    } finally {
      this.simulating.set(false);
    }
  }

  onSimPeriodChange(value: string): void {
    this.simPeriod.set(value);
  }

  async onPayslipPeriodChange(value: string): Promise<void> {
    this.payslipPeriod.set(value);
    await this.loadPayslips();
  }

  // ── Process run ──
  openProcessModal(): void {
    this.processPeriod.set(this.simPeriod());
    this.showProcessModal.set(true);
  }

  async process(): Promise<void> {
    this.processing.set(true);
    try {
      await this.api.processPayrollRun(this.processPeriod());
      this.toast.success(`Payroll for ${this.processPeriod()} calculated.`);
      this.showProcessModal.set(false);
      await this.loadRuns();
      // Refresh the payslip view and the simulation for the same period.
      if (this.processPeriod() === this.payslipPeriod()) await this.loadPayslips();
      if (this.processPeriod() === this.simPeriod()) await this.simulate();
    } catch {
      this.toast.error('Failed to process the payroll run.');
    } finally {
      this.processing.set(false);
    }
  }

  // ── Run lifecycle actions ──
  isLocked(run: PayrollRunDto): boolean {
    return this.LOCKED_STATUSES.includes(run.status);
  }

  nextStatusLabel(status: string): string {
    switch (status) {
      case 'Draft':
      case 'Calculated':
        return 'HR Review';
      case 'HR Review':
        return 'Finance Review';
      case 'Finance Review':
        return 'Finalize';
      default:
        return '';
    }
  }

  canAdvance(run: PayrollRunDto): boolean {
    return !this.isLocked(run);
  }

  async advance(run: PayrollRunDto): Promise<void> {
    try {
      await this.api.advanceStatus(run.id);
      this.toast.success(`Run moved to ${this.nextStatusLabel(run.status)}.`);
      await this.loadRuns();
    } catch {
      this.toast.error('Could not advance the run.');
    }
  }

  async reopen(run: PayrollRunDto): Promise<void> {
    const ok = await this.dialog.confirm({
      title: 'Reopen payroll run',
      message: `Reopen the ${run.period} run? It will return to Draft so it can be recalculated.`,
      confirmText: 'Reopen',
      type: 'warning'
    });
    if (!ok) return;
    try {
      await this.api.reopen(run.id);
      this.toast.success('Run reopened.');
      await this.loadRuns();
    } catch {
      this.toast.error('Could not reopen the run.');
    }
  }

  // ── Anomalies ──
  async resolveAnomaly(a: PayrollAnomalyDto): Promise<void> {
    const note = await this.dialog.prompt({
      title: 'Resolve Anomaly',
      message: `Resolution note for "${a.kind}" (${a.employeeName})?`,
      placeholder: 'Resolution Note',
      confirmText: 'Submit',
      type: 'info',
      icon: 'pi-check-circle'
    });
    if (note === null) return;
    try {
      await this.api.resolveAnomaly(a.id, note || '');
      this.toast.success('Anomaly marked as reviewed.');
      await this.simulate();
      await this.loadRuns();
    } catch {
      this.toast.error('Could not resolve the anomaly.');
    }
  }

  // ── Payslip detail ──
  viewPayslip(p: PayslipDto): void {
    this.currentPayslip = p;
    this.showPayslipModal.set(true);
  }

  // ── Presentation helpers ──
  expanded = signal<Set<string>>(new Set());
  toggleEmployee(id: string): void {
    const set = new Set(this.expanded());
    if (set.has(id)) set.delete(id);
    else set.add(id);
    this.expanded.set(set);
  }
  isExpanded(id: string): boolean {
    return this.expanded().has(id);
  }

  openAnomalies = computed(() =>
    (this.preview()?.anomalies ?? []).filter(a => !a.isResolved)
  );

  statusClass(status: string): string {
    switch (status) {
      case 'Draft':
      case 'Calculated':
        return 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300';
      case 'HR Review':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300';
      case 'Finance Review':
        return 'bg-violet-100 text-violet-700 dark:bg-violet-900/40 dark:text-violet-300';
      case 'Finalized':
        return 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/40 dark:text-indigo-300';
      case 'Posted':
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';
      default:
        return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
    }
  }

  severityClass(severity: string): string {
    return severity === 'Critical'
      ? 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300'
      : 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300';
  }

  anomalyIcon(kind: string): string {
    switch (kind) {
      case 'NegativeNet':
        return 'pi-exclamation-triangle';
      case 'ZeroNet':
        return 'pi-minus-circle';
      case 'SalarySpike':
        return 'pi-arrow-up-right';
      case 'HighOvertime':
        return 'pi-clock';
      case 'MissingAttendance':
        return 'pi-calendar-times';
      case 'TerminatedButPaid':
        return 'pi-ban';
      case 'DuplicateEmployee':
        return 'pi-copy';
      case 'HighBonus':
        return 'pi-gift';
      case 'UnexpectedDeduction':
        return 'pi-minus';
      default:
        return 'pi-flag';
    }
  }

  money(value: number): string {
    return (value ?? 0).toLocaleString(undefined, { maximumFractionDigits: 0 });
  }

  deltaClass(value: number): string {
    if (value > 0) return 'text-emerald-600 dark:text-emerald-400';
    if (value < 0) return 'text-rose-600 dark:text-rose-400';
    return 'text-slate-500 dark:text-slate-400';
  }
}
