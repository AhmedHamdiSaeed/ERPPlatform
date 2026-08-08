import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Employee } from '../../../core/models/erp-models';
import { MOCK_EMPLOYEES } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent {
  employees = signal<Employee[]>(MOCK_EMPLOYEES);
  searchQuery = '';
  statusFilter = 'ALL';

  showModal = signal(false);
  isEditMode = false;

  currentEmp: Partial<Employee> = {};

  filteredEmployees() {
    return this.employees().filter(e => {
      const matchesSearch = !this.searchQuery || 
        e.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        e.email.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        e.employeeCode.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchesStatus = this.statusFilter === 'ALL' || e.status === this.statusFilter;
      return matchesSearch && matchesStatus;
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentEmp = {
      id: `emp-${Date.now()}`,
      employeeCode: `EMP-0${Math.floor(100 + Math.random() * 900)}`,
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
      status: 'Active',
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
    } else {
      this.employees.update(list => [this.currentEmp as Employee, ...list]);
    }
    this.showModal.set(false);
  }

  deleteEmployee(id: string) {
    if (confirm('Are you sure you want to remove this employee record?')) {
      this.employees.update(list => list.filter(e => e.id !== id));
    }
  }

  exportCsv() {
    alert('Employee roster exported to CSV successfully!');
  }
}
