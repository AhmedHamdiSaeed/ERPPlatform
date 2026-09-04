import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface EmployeeContract {
  id: string;
  employeeName: string;
  employeeCode: string;
  contractType: 'Full-Time' | 'Part-Time' | 'Contract' | 'Internship';
  startDate: string;
  endDate: string;
  monthlySalary: number;
  status: 'Active' | 'Pending Renewal' | 'Expired';
}

@Component({
  selector: 'app-contracts',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './contracts.component.html'
})
export class ContractsComponent {
  private toast = inject(ToastService);

  contracts = signal<EmployeeContract[]>([
    { id: 'cnt-1', employeeName: 'Ahmed Hamdi', employeeCode: 'EMP-0101', contractType: 'Full-Time', startDate: '2021-03-15', endDate: '2027-03-14', monthlySalary: 14500, status: 'Active' },
    { id: 'cnt-2', employeeName: 'Sara Mahmoud', employeeCode: 'EMP-0102', contractType: 'Full-Time', startDate: '2020-01-10', endDate: '2026-12-31', monthlySalary: 11200, status: 'Active' },
    { id: 'cnt-3', employeeName: 'Khaled Hassan', employeeCode: 'EMP-0103', contractType: 'Contract', startDate: '2025-06-01', endDate: '2026-09-01', monthlySalary: 13000, status: 'Pending Renewal' }
  ]);

  showModal = signal(false);
  newCnt: Partial<EmployeeContract> = { contractType: 'Full-Time', status: 'Active' };

  openAddModal() {
    this.newCnt = { employeeName: '', employeeCode: 'EMP-0108', contractType: 'Full-Time', startDate: '2026-09-01', endDate: '2028-09-01', monthlySalary: 10000, status: 'Active' };
    this.showModal.set(true);
  }

  saveContract() {
    if (!this.newCnt.employeeName) {
      this.toast.warning('Please enter the employee name.');
      return;
    }
    this.contracts.update(list => [{ ...this.newCnt, id: `cnt-${Date.now()}` } as EmployeeContract, ...list]);
    this.toast.success('Employee contract recorded.');
    this.showModal.set(false);
  }
}
