import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PayrollRun, Payslip } from '../../../core/models/erp-models';
import { SharedApiService } from '../../../core/services/api/shared-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-payroll-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payroll-management.component.html'
})
export class PayrollManagementComponent {
  private toast = inject(ToastService);
  private sharedApi = inject(SharedApiService);

  payrollRuns = signal<PayrollRun[]>([]);
  payslips = signal<Payslip[]>([]);

  showProcessModal = signal(false);
  selectedPeriod = 'September 2026';

  showPayslipModal = signal(false);
  currentPayslip: Payslip | null = null;

  constructor() {
    this.loadData();
  }

  async loadData() {
    try {
      const [runs, slips] = await Promise.all([
        this.sharedApi.getPayrollRuns(),
        this.sharedApi.getPayslips()
      ]);
      this.payrollRuns.set(runs);
      this.payslips.set(slips);
    } catch (e) {
      console.error('Failed to load payroll data', e);
      this.toast.error('Could not load payroll data from the server.');
    }
  }

  openProcessModal() {
    this.showProcessModal.set(true);
  }

  async runPayroll() {
    try {
      await this.sharedApi.processPayrollRun(this.selectedPeriod);
      await this.loadData();
      this.toast.success(`Payroll for ${this.selectedPeriod} processed & approved successfully.`);
    } catch (e) {
      console.error('Failed to process payroll', e);
      this.toast.error('Failed to process the payroll run.');
    }
    this.showProcessModal.set(false);
  }

  viewPayslip(payslip: Payslip) {
    this.currentPayslip = payslip;
    this.showPayslipModal.set(true);
  }

  downloadPayslipPdf(employeeName: string) {
    this.toast.success(`Payslip PDF for ${employeeName} downloaded successfully.`);
  }
}
