import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Employee } from '../../../core/models/erp-models';
import { MOCK_EMPLOYEES } from '../../../core/mock/mock-data';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent {
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  employees = signal<Employee[]>(MOCK_EMPLOYEES);
  searchQuery = '';
  statusFilter = 'ALL';
  departmentFilter = 'ALL';

  showModal = signal(false);
  isEditMode = false;
  currentEmp: Partial<Employee> = {};

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
      id: `emp-${Date.now()}`,
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

  saveEmployee() {
    if (this.isEditMode) {
      this.employees.update(list => list.map(e => e.id === this.currentEmp.id ? { ...e, ...this.currentEmp } as Employee : e));
      this.toast.success('Employee profile updated successfully.');
    } else {
      this.employees.update(list => [this.currentEmp as Employee, ...list]);
      this.toast.success('New employee registered successfully.');
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
      this.employees.update(list => list.filter(e => e.id !== id));
      this.toast.success('Employee record deleted.');
    }
  }

  exportCsv() {
    this.toast.success('Employee roster exported to CSV file.');
  }
}
