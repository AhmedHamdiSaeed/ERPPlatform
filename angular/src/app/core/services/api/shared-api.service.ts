import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { PayrollRun, Payslip, Deal, Account, JournalEntry, AuditLogEntry } from '../../models/erp-models';

interface PayrollRunDto extends AbpEntity {
  period: string; totalEmployees: number; totalGrossSalary: number;
  totalDeductions: number; totalNetSalary: number; status: string; processedDate: string;
}

interface PayslipDto extends AbpEntity {
  employeeId: string; employeeName: string; period: string;
  baseSalary: number; allowances: number; deductions: number; netSalary: number; status: string;
}

interface DealDto extends AbpEntity {
  title: string; customerName: string; value: number; stage: string;
  probability: number; expectedCloseDate: string; ownerName: string;
}

interface AccountDto extends AbpEntity {
  code: string; name: string; type: string; balance: number;
  currency: string; parentCode?: string; isActive: boolean;
}

interface JournalEntryDto extends AbpEntity {
  entryNumber: string; entryDate: string; description: string;
  totalDebit: number; totalCredit: number; status: string; createdBy: string;
}

interface AuditLogEntryDto extends AbpEntity {
  entityName: string; entityId: string; action: string;
  userName: string; timestamp: string; changesJson: string;
}

@Injectable({ providedIn: 'root' })
export class SharedApiService extends ErpApiService {
  getPayrollRuns(): Promise<PayrollRun[]> {
    return this.getList<PayrollRunDto>('payroll/payroll-runs').then(items =>
      items.map(r => ({ ...r, processedDate: toDateString(r.processedDate) })) as PayrollRun[]
    );
  }

  getPayslips(): Promise<Payslip[]> {
    return this.getList<PayslipDto>('payroll/payslips').then(items => items as Payslip[]);
  }

  processPayrollRun(period: string): Promise<PayrollRun> {
    return this.post<PayrollRunDto>(`payroll/process-payroll-run?period=${encodeURIComponent(period)}`, {})
      .then(r => ({ ...r, processedDate: toDateString(r.processedDate) })) as Promise<PayrollRun>;
  }

  getDeals(): Promise<Deal[]> {
    return this.getList<DealDto>('crm/deals').then(items =>
      items.map(d => ({ ...d, expectedCloseDate: toDateString(d.expectedCloseDate) })) as Deal[]
    );
  }

  createDeal(deal: Partial<Deal>): Promise<void> {
    return this.post('crm/deals', deal);
  }

  updateDealStage(id: string, newStage: string): Promise<void> {
    return this.put(`crm/${id}/update-deal-stage?newStage=${encodeURIComponent(newStage)}`, {});
  }

  getAccounts(): Promise<Account[]> {
    return this.getList<AccountDto>('finance/accounts').then(items => items as Account[]);
  }

  getJournalEntries(): Promise<JournalEntry[]> {
    return this.getList<JournalEntryDto>('finance/journal-entries').then(items =>
      items.map(j => ({ ...j, entryDate: toDateString(j.entryDate) })) as JournalEntry[]
    );
  }

  createJournalEntry(entry: Partial<JournalEntry>): Promise<void> {
    return this.post('finance/journal-entry', {
      entryNumber: entry.entryNumber,
      description: entry.description,
      totalDebit: entry.totalDebit ?? 0,
      totalCredit: entry.totalCredit ?? 0,
      createdBy: entry.createdBy
    });
  }

  getAuditLogs(): Promise<AuditLogEntry[]> {
    return this.getList<AuditLogEntryDto>('audit-log/audit-logs').then(items =>
      items.map(l => ({
        ...l,
        timestamp: new Date(l.timestamp).toISOString().replace('T', ' ').slice(0, 19)
      })) as AuditLogEntry[]
    );
  }
}
