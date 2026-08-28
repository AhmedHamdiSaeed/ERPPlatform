import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface SystemRole {
  id: string;
  name: string;
  description: string;
  userCount: number;
}

@Component({
  selector: 'app-roles-permissions',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-permissions.component.html'
})
export class RolesPermissionsComponent {
  private toast = inject(ToastService);

  roles = signal<SystemRole[]>([
    { id: 'r-1', name: 'Admin', description: 'Unrestricted full enterprise administrative access', userCount: 2 },
    { id: 'r-2', name: 'HR Manager', description: 'Employee roster, attendance, leave approvals & payroll', userCount: 5 },
    { id: 'r-3', name: 'Inventory Manager', description: 'Products catalog, stock transfers & purchase orders', userCount: 8 },
    { id: 'r-4', name: 'Employee', description: 'Standard self-service profile, leave application & chat', userCount: 230 }
  ]);

  selectedRoleId = signal<string>('r-1');

  permissions = [
    { group: 'HR & Workforce', items: ['View Employees', 'Manage Employees', 'Approve Leaves', 'Process Payroll'] },
    { group: 'Inventory & Supply Chain', items: ['View Products', 'Manage Catalog', 'Approve Stock Transfers', 'Create POs'] },
    { group: 'Finance & Accounting', items: ['View Ledger', 'Post Journal Entries', 'Manage Accounts'] },
    { group: 'Sales & Billing', items: ['Create Invoices', 'Manage Quotations', 'View Sales Analytics'] },
    { group: 'System & Security', items: ['Manage Users', 'Configure System Settings', 'View Audit Logs'] }
  ];

  selectRole(id: string) {
    this.selectedRoleId.set(id);
  }

  savePermissions() {
    this.toast.success('Role permission policy updated successfully.');
  }
}
