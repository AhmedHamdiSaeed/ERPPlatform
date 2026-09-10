import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ErpApiService } from './erp-api.service';

// ─────────────────────────────────────────────────────────────
// Payroll DTOs — mirror Shared/ERPPlatform.Application/Payroll
// ─────────────────────────────────────────────────────────────

export interface PayrollLineDto {
  code: string;
  name: string;
  type: string; // Earning | Deduction | EmployerContribution
  amount: number;
  isTaxable: boolean;
}

export interface PayrollRunDto {
  id: string;
  period: string;
  totalEmployees: number;
  totalGrossSalary: number;
  totalAllowances: number;
  totalOvertime: number;
  totalTax: number;
  totalDeductions: number;
  totalNetSalary: number;
  totalEmployerCost: number;
  status: string; // Draft | Calculated | HR Review | Finance Review | Finalized | Posted
  processedDate?: string | null;
  lockedAt?: string | null;
  approvedBy?: string | null;
  notes?: string | null;
}

export interface PayslipDto {
  id: string;
  employeeId: string;
  employeeName: string;
  period: string;
  baseSalary: number;
  allowances: number;
  overtimeAmount: number;
  grossSalary: number;
  taxAmount: number;
  deductions: number;
  netSalary: number;
  employerCost: number;
  department: string;
  status: string;
  payrollRunId?: string | null;
}

export interface PayrollEmployeeResultDto {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  department: string;
  basic: number;
  allowances: number;
  overtimeAmount: number;
  grossSalary: number;
  taxAndInsurance: number;
  deductions: number;
  netSalary: number;
  employerCost: number;
  workingHours: number;
  overtimeHours: number;
  unpaidLeaveDays: number;
  previousNetSalary: number;
  netDelta: number;
  netDeltaPercentage: number;
  lines: PayrollLineDto[];
}

export interface PayrollTotalsDto {
  employeeCount: number;
  grossSalary: number;
  allowances: number;
  overtime: number;
  taxAndInsurance: number;
  deductions: number;
  netSalary: number;
  employerCost: number;
}

export interface PayrollAnomalyDto {
  id: string;
  period: string;
  employeeId?: string | null;
  employeeName: string;
  kind: string;
  severity: string; // Warning | Critical
  message: string;
  delta: number;
  isResolved: boolean;
  resolutionNote?: string | null;
}

export interface PayrollPreviewDto {
  period: string;
  periodStart: string;
  periodEnd: string;
  employees: PayrollEmployeeResultDto[];
  totals: PayrollTotalsDto;
  previousTotals?: PayrollTotalsDto | null;
  anomalies: PayrollAnomalyDto[];
  netDelta: number;
  grossDelta: number;
  headcountDelta: number;
  componentCount: number;
  usingFallback: boolean;
  hasPreviousRun: boolean;
}

export interface SalaryComponentDto {
  id: string;
  code: string;
  name: string;
  type: string; // Earning | Deduction | EmployerContribution
  calculationType: string; // Fixed | Percentage | Formula
  amount: number;
  percentageOfComponentCode: string;
  formula: string;
  isTaxable: boolean;
  isRecurring: boolean;
  countsAsEmployerCost: boolean;
  isActive: boolean;
  effectiveFrom: string;
  effectiveTo?: string | null;
  glAccount: string;
  costCenter: string;
  sortOrder: number;
}

export interface EmployeeSalaryComponentDto {
  id: string;
  employeeId: string;
  employeeName: string;
  componentId: string;
  componentCode: string;
  amount: number;
  isActive: boolean;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note: string;
}

function currentPeriod(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}

@Injectable({ providedIn: 'root' })
export class PayrollApiService extends ErpApiService {
  /** "yyyy-MM" for the current month, the canonical payroll period key. */
  readonly defaultPeriod = currentPeriod();

  // ── Runs ─────────────────────────────────────────────────────
  getPayrollRuns(): Promise<PayrollRunDto[]> {
    return this.getList<PayrollRunDto>('payroll/payroll-runs');
  }

  processPayrollRun(period: string): Promise<PayrollRunDto> {
    return this.post<PayrollRunDto>(
      `payroll/process-payroll-run?period=${encodeURIComponent(period)}`,
      {}
    );
  }

  advanceStatus(id: string): Promise<PayrollRunDto> {
    return this.post<PayrollRunDto>(`payroll/${id}/advance-status`, {});
  }

  reopen(id: string): Promise<PayrollRunDto> {
    return this.post<PayrollRunDto>(`payroll/${id}/reopen`, {});
  }

  // ── Simulation preview (no writes) ──────────────────────────
  getPreview(period: string): Promise<PayrollPreviewDto> {
    return this.get<PayrollPreviewDto>(
      `payroll/preview?period=${encodeURIComponent(period)}`
    );
  }

  // ── Payslips ───────────────────────────────────────────────
  getPayslips(period?: string): Promise<PayslipDto[]> {
    const q = period ? `?Period=${encodeURIComponent(period)}` : '';
    return this.getList<PayslipDto>(`payroll/payslips${q}`);
  }

  // ── Anomalies (Issues to Review) ────────────────────────────
  getAnomalies(period: string): Promise<PayrollAnomalyDto[]> {
    return this.getList<PayrollAnomalyDto>(
      `payroll/anomalies?period=${encodeURIComponent(period)}`
    );
  }

  resolveAnomaly(id: string, note: string): Promise<PayrollAnomalyDto> {
    return this.post<PayrollAnomalyDto>(`payroll/${id}/resolve-anomaly`, { note });
  }

  // ── Salary components (the rule builder surface) ────────────
  getSalaryComponents(): Promise<SalaryComponentDto[]> {
    return this.getList<SalaryComponentDto>('salary-component');
  }

  createSalaryComponent(c: Partial<SalaryComponentDto>): Promise<SalaryComponentDto> {
    return this.post<SalaryComponentDto>('salary-component', c);
  }

  updateSalaryComponent(id: string, c: Partial<SalaryComponentDto>): Promise<SalaryComponentDto> {
    return this.put<SalaryComponentDto>(`salary-component/${id}`, c);
  }

  deleteSalaryComponent(id: string): Promise<void> {
    return this.delete(`salary-component/${id}`);
  }
}

export { currentPeriod };
