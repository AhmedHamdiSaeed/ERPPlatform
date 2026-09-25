import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { BenefitPlan, EmployeeBenefit, Employee } from '../../../core/models/erp-models';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-benefits',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './benefits.component.html'
})
export class BenefitsComponent implements OnInit {
  private hrApi = inject(HrApiService);
  state = inject(StateService);

  activeTab: 'plans' | 'roster' = 'plans';
  loading = false;
  plans: BenefitPlan[] = [];
  benefits: EmployeeBenefit[] = [];
  employees: Employee[] = [];

  // Plan modal
  showNewPlanModal = false;
  newPlan: Partial<BenefitPlan> = {
    planCode: '',
    planName: '',
    category: 'MedicalInsurance',
    providerName: '',
    coverageDetails: '',
    employerContributionMonthly: 200,
    employeeContributionMonthly: 50
  };

  // Enrollment modal
  showEnrollModal = false;
  enrollForm = {
    employeeId: '',
    benefitPlanId: '',
    coverageAmount: 50000
  };

  get activeBenefitsCount(): number {
    return this.benefits.filter(b => b.status === 'Active').length;
  }

  ngOnInit(): void {
    this.loadData();
  }

  async loadData(): Promise<void> {
    this.loading = true;
    try {
      const [pls, bns, emps] = await Promise.all([
        this.hrApi.getBenefitPlans(),
        this.hrApi.getEmployeeBenefits(),
        this.hrApi.getEmployees()
      ]);
      this.plans = pls || [];
      this.benefits = bns || [];
      this.employees = emps || [];
    } catch (err) {
      console.error('Error loading benefits data', err);
    } finally {
      this.loading = false;
    }
  }

  openEnrollModal(plan?: BenefitPlan): void {
    this.enrollForm = {
      benefitPlanId: plan?.id || (this.plans.length > 0 ? this.plans[0].id : ''),
      employeeId: this.employees.length > 0 ? this.employees[0].id : '',
      coverageAmount: 50000
    };
    this.showEnrollModal = true;
  }

  async createPlan(): Promise<void> {
    if (!this.newPlan.planName) return;
    try {
      await this.hrApi.createBenefitPlan(this.newPlan);
      this.showNewPlanModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error creating plan', err);
    }
  }

  async enroll(): Promise<void> {
    if (!this.enrollForm.benefitPlanId || !this.enrollForm.employeeId) return;
    try {
      await this.hrApi.enrollEmployeeBenefit(this.enrollForm);
      this.showEnrollModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error enrolling benefit', err);
    }
  }

  async terminate(benefitId: string): Promise<void> {
    try {
      await this.hrApi.terminateBenefit(benefitId);
      this.loadData();
    } catch (err) {
      console.error('Error terminating benefit', err);
    }
  }
}
