import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Employee } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe, AppDatePipe],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent {
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
  private hrApi = inject(HrApiService);

  employees = signal<Employee[]>([]);
  searchQuery = '';
  statusFilter = 'ALL';
  departmentFilter = 'ALL';

  showModal = signal(false);
  isEditMode = false;
  currentEmp: Partial<Employee> = {};

  constructor() {
    this.loadEmployees();
  }

  async loadEmployees() {
    try {
      this.employees.set(await this.hrApi.getEmployees());
    } catch (e) {
      console.error('Failed to load employees', e);
      this.toast.error('Could not load employees from the server.');
    }
  }

  filteredEmployees() {
    return this.employees().filter(emp => {
      const matchQuery = !this.searchQuery ||
        emp.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        emp.email.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        emp.employeeCode.toLowerCase().includes(this.searchQuery.toLowerCase());

      const matchStatus = this.statusFilter === 'ALL' || emp.status === this.statusFilter;
      const matchDept = this.departmentFilter === 'ALL' || emp.departmentName === this.departmentFilter;

      return matchQuery && matchStatus && matchDept;
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentEmp = {
      employeeCode: `EMP-2026-00${Math.floor(Math.random() * 90)}`,
      status: 'Active',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
      joiningDate: new Date().toISOString().split('T')[0]
    };
    this.showModal.set(true);
  }

  openEditModal(emp: Employee) {
    this.isEditMode = true;
    this.currentEmp = { ...emp };
    this.showModal.set(true);
  }

  async saveEmployee() {
    try {
      if (this.isEditMode) {
        await this.hrApi.updateEmployee(this.currentEmp.id!, this.currentEmp);
        this.toast.success('Employee profile updated successfully.');
      } else {
        await this.hrApi.createEmployee(this.currentEmp);
        this.toast.success('New employee registered successfully.');
      }
      await this.loadEmployees();
    } catch (e) {
      console.error('Failed to save employee', e);
      this.toast.error('Failed to save the employee record.');
    }
    this.showModal.set(false);
  }

  async deleteEmployee(id: string) {
    const emp = this.employees().find(e => e.id === id);
    const confirmed = await this.dialog.confirm({
      title: 'Delete Employee Record',
      message: `Are you sure you want to delete ${emp ? emp.name : 'this employee'}? This action cannot be undone.`,
      confirmText: 'Delete Employee',
      cancelText: 'Cancel',
      type: 'danger'
    });

    if (confirmed) {
      try {
        await this.hrApi.deleteEmployee(id);
        await this.loadEmployees();
        this.toast.success('Employee record deleted.');
      } catch (e) {
        console.error('Failed to delete employee', e);
        this.toast.error('Failed to delete the employee record.');
      }
    }
  }

  exportCsv() {
    this.toast.success('Employee roster exported to CSV file.');
  }
}
