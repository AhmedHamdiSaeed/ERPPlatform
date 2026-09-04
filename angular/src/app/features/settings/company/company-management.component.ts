import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { OrgApiService, Company, Branch, CostCenter, FiscalYear } from '../../../core/services/api/org-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-company-management',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './company-management.component.html'
})
export class CompanyManagementComponent {
  private orgApi = inject(OrgApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  activeTab = signal<'companies' | 'branches' | 'costCenters' | 'fiscalYears'>('companies');

  companies = signal<Company[]>([]);
  branches = signal<Branch[]>([]);
  costCenters = signal<CostCenter[]>([]);
  fiscalYears = signal<FiscalYear[]>([]);

  showModal = signal(false);
  isEditMode = false;

  currentCompany: Partial<Company> = {};
  currentBranch: Partial<Branch> = {};
  currentCostCenter: Partial<CostCenter> = {};
  currentFiscalYear: Partial<FiscalYear> = {};

  /**
   * Whole-sentence key for the creation modal heading. Returning a single key
   * (instead of "Add {{ activeTab() }} Record") keeps the heading translatable
   * as one phrase, which is required for correct Arabic word order.
   */
  private readonly addRecordTitleKeys: Record<string, string> = {
    companies: 'Add Company Record',
    branches: 'Add Branch Record',
    costCenters: 'Add Cost Center Record',
    fiscalYears: 'Add Fiscal Year Record'
  };

  addRecordTitleKey(): string {
    return this.addRecordTitleKeys[this.activeTab()] ?? 'Add Record';
  }

  constructor() {
    this.loadAllData();
  }

  async loadAllData() {
    try {
      const [c, b, cc, fy] = await Promise.all([
        this.orgApi.getCompanies(),
        this.orgApi.getBranches(),
        this.orgApi.getCostCenters(),
        this.orgApi.getFiscalYears()
      ]);
      this.companies.set(c);
      this.branches.set(b);
      this.costCenters.set(cc);
      this.fiscalYears.set(fy);
    } catch (e) {
      console.error('Error loading org data', e);
    }
  }

  openAddModal() {
    this.isEditMode = false;
    if (this.activeTab() === 'companies') {
      this.currentCompany = { name: '', taxNumber: '', email: '', phone: '', country: 'Egypt', currency: 'USD', isActive: true };
    } else if (this.activeTab() === 'branches') {
      this.currentBranch = { name: '', code: `BR-${Math.floor(Math.random() * 900 + 100)}`, companyName: 'ERP Enterprise Group HQ', address: '', isActive: true };
    } else if (this.activeTab() === 'costCenters') {
      this.currentCostCenter = { code: `CC-${Math.floor(Math.random() * 900 + 100)}`, name: '', description: '', isActive: true };
    } else if (this.activeTab() === 'fiscalYears') {
      this.currentFiscalYear = { name: 'FY 2027', startDate: '2027-01-01', endDate: '2027-12-31', isCurrent: false, isClosed: false };
    }
    this.showModal.set(true);
  }

  async saveItem() {
    try {
      if (this.activeTab() === 'companies') {
        await this.orgApi.createCompany(this.currentCompany);
        this.toast.success('Company profile saved.');
      } else if (this.activeTab() === 'branches') {
        await this.orgApi.createBranch(this.currentBranch);
        this.toast.success('Branch added.');
      } else if (this.activeTab() === 'costCenters') {
        await this.orgApi.createCostCenter(this.currentCostCenter);
        this.toast.success('Cost center created.');
      } else if (this.activeTab() === 'fiscalYears') {
        await this.orgApi.createFiscalYear(this.currentFiscalYear);
        this.toast.success('Fiscal year defined.');
      }
      await this.loadAllData();
    } catch (e) {
      this.toast.error('Failed to save record.');
    }
    this.showModal.set(false);
  }

  async setAsCurrentFiscalYear(id: string) {
    await this.orgApi.setCurrentFiscalYear(id);
    this.toast.success('Active fiscal year updated.');
    await this.loadAllData();
  }
}
