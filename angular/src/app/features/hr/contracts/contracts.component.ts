import { Component, inject, signal, OnInit } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { Employee, EmployeeContract } from '../../../core/models/erp-models';

@Component({
  selector: 'app-contracts',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './contracts.component.html'
})
export class ContractsComponent implements OnInit {
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  contracts = signal<EmployeeContract[]>([]);
  employees = signal<Employee[]>([]);
  loading = signal(false);

  showModal = signal(false);
  newCnt: Partial<EmployeeContract> = {
    contractType: 'Permanent',
    startDate: new Date().toISOString().substring(0, 10),
    basicSalary: 0,
    housingAllowance: 0,
    transportationAllowance: 0,
    otherAllowances: 0,
    workingHoursPerWeek: 40,
    noticePeriodDays: 30,
    status: 'Active'
  };

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.loading.set(true);
    try {
      const [ctrs, emps] = await Promise.all([
        this.hrApi.getContracts(),
        this.hrApi.getEmployees()
      ]);
      this.contracts.set(ctrs);
      this.employees.set(emps);
    } catch (e) {
      console.error('Failed to load contracts', e);
      this.toast.error('Failed to load contracts from server.');
    } finally {
      this.loading.set(false);
    }
  }

  onEmployeeSelect(empId: string) {
    const emp = this.employees().find(e => e.id === empId);
    if (emp) {
      this.newCnt.employeeId = emp.id;
      this.newCnt.employeeName = emp.name;
      this.newCnt.basicSalary = emp.salary || 0;
    }
  }

  openAddModal() {
    this.newCnt = {
      contractType: 'Permanent',
      startDate: new Date().toISOString().substring(0, 10),
      basicSalary: 0,
      housingAllowance: 0,
      transportationAllowance: 0,
      otherAllowances: 0,
      workingHoursPerWeek: 40,
      noticePeriodDays: 30,
      status: 'Active'
    };
    if (this.employees().length > 0) {
      this.onEmployeeSelect(this.employees()[0].id);
    }
    this.showModal.set(true);
  }

  async saveContract() {
    if (!this.newCnt.employeeId || !this.newCnt.employeeName) {
      this.toast.warning('Please select an employee.');
      return;
    }

    try {
      await this.hrApi.createContract(this.newCnt);
      this.toast.success('Employment contract created successfully.');
      this.showModal.set(false);
      await this.loadData();
    } catch (e) {
      console.error('Failed to save contract', e);
      this.toast.error('Failed to create contract.');
    }
  }
}
