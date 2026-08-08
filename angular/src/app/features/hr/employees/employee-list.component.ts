import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Employee } from '../../../core/models/erp-models';
import { MOCK_EMPLOYEES } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">HR Employees Directory</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">Manage personnel profiles, positions, department assignments, and status.</p>
        </div>

        <div class="flex items-center gap-2">
          <button (click)="openAddModal()" class="btn-primary text-xs cursor-pointer">
            <i class="pi pi-user-plus"></i> Add Employee
          </button>
          <button (click)="exportCsv()" class="btn-outline text-xs cursor-pointer">
            <i class="pi pi-file-excel"></i> Export CSV
          </button>
        </div>
      </div>

      <!-- Filters & Search Bar -->
      <div class="card-panel flex flex-col sm:flex-row items-center justify-between gap-3">
        <div class="relative w-full sm:w-80">
          <i class="pi pi-search absolute left-3 top-3 text-slate-400 text-xs"></i>
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            placeholder="Search by name, email, code or role..."
            class="w-full pl-9 pr-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-[var(--text-main)] focus:outline-hidden"
          />
        </div>

        <div class="flex items-center gap-2 w-full sm:w-auto overflow-x-auto">
          <select [(ngModel)]="statusFilter" class="px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-[var(--text-main)]">
            <option value="ALL">All Statuses</option>
            <option value="Active">Active</option>
            <option value="On Leave">On Leave</option>
            <option value="Inactive">Inactive</option>
          </select>

          <span class="text-xs text-[var(--text-muted)] font-semibold whitespace-nowrap">
            Showing {{ filteredEmployees().length }} Employees
          </span>
        </div>
      </div>

      <!-- Employee Table -->
      <div class="card-panel !p-0 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-100/70 dark:bg-slate-800/70 text-slate-500 dark:text-slate-400 text-[11px] font-bold uppercase tracking-wider border-b border-[var(--border-color)]">
                <th class="p-3.5">Employee</th>
                <th class="p-3.5">ID Code</th>
                <th class="p-3.5">Department</th>
                <th class="p-3.5">Position</th>
                <th class="p-3.5">Joining Date</th>
                <th class="p-3.5">Status</th>
                <th class="p-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border-color)] text-xs text-[var(--text-main)]">
              @for (emp of filteredEmployees(); track emp.id) {
                <tr class="hover:bg-slate-50/80 dark:hover:bg-slate-800/50 transition-colors">
                  
                  <!-- Avatar & Name -->
                  <td class="p-3.5">
                    <a [routerLink]="['/hr/employees', emp.id]" class="flex items-center gap-3 decoration-none group">
                      <img [src]="emp.avatar" [alt]="emp.name" class="w-9 h-9 rounded-full object-cover ring-2 ring-blue-500/20 group-hover:ring-blue-500 transition-all" />
                      <div>
                        <span class="font-bold block text-[var(--text-main)] group-hover:text-blue-600 transition-colors">{{ emp.name }}</span>
                        <span class="text-[11px] text-[var(--text-muted)] block">{{ emp.email }}</span>
                      </div>
                    </a>
                  </td>

                  <td class="p-3.5 font-mono text-[11px] font-semibold text-slate-500">{{ emp.employeeCode }}</td>
                  <td class="p-3.5 font-medium">{{ emp.departmentName }}</td>
                  <td class="p-3.5 text-slate-600 dark:text-slate-300">{{ emp.position }}</td>
                  <td class="p-3.5 text-slate-500">{{ emp.joiningDate }}</td>

                  <td class="p-3.5">
                    <span class="status-badge" [class.active]="emp.status === 'Active'" [class.on_leave]="emp.status === 'On Leave'" [class.inactive]="emp.status === 'Inactive'">
                      {{ emp.status }}
                    </span>
                  </td>

                  <td class="p-3.5 text-right space-x-1">
                    <a [routerLink]="['/hr/employees', emp.id]" class="p-1.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-slate-800 rounded-md inline-block" title="View Profile">
                      <i class="pi pi-eye"></i>
                    </a>
                    <button (click)="openEditModal(emp)" class="p-1.5 text-slate-600 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-md" title="Edit">
                      <i class="pi pi-pencil"></i>
                    </button>
                    <button (click)="deleteEmployee(emp.id)" class="p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-slate-800 rounded-md" title="Delete">
                      <i class="pi pi-trash"></i>
                    </button>
                  </td>

                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Add/Edit Employee Dialog -->
      @if (showModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-fade-in">
          <div class="w-full max-w-lg bg-[var(--bg-card)] border border-[var(--border-color)] rounded-2xl shadow-2xl overflow-hidden p-6 space-y-4">
            <div class="flex items-center justify-between border-b border-[var(--border-color)] pb-3">
              <h3 class="font-bold text-sm text-[var(--text-main)]">{{ isEditMode ? 'Edit Employee Profile' : 'Add New Employee' }}</h3>
              <button (click)="showModal.set(false)" class="text-slate-400 hover:text-slate-600"><i class="pi pi-times"></i></button>
            </div>

            <div class="grid grid-cols-2 gap-3 text-xs">
              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Full Name</label>
                <input type="text" [(ngModel)]="currentEmp.name" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
              </div>
              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Email Address</label>
                <input type="email" [(ngModel)]="currentEmp.email" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
              </div>
              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Department</label>
                <input type="text" [(ngModel)]="currentEmp.departmentName" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
              </div>
              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Position</label>
                <input type="text" [(ngModel)]="currentEmp.position" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
              </div>
            </div>

            <div class="pt-3 border-t border-[var(--border-color)] flex justify-end gap-2">
              <button (click)="showModal.set(false)" class="btn-outline text-xs">Cancel</button>
              <button (click)="saveEmployee()" class="btn-primary text-xs">Save Employee</button>
            </div>
          </div>
        </div>
      }

    </div>
  `
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
