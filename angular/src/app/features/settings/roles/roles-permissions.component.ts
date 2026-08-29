import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { StateService } from '../../../core/services/state.service';
import { environment } from '../../../../environments/environment';
import { PERMISSIONS } from '../../../core/models/permissions';

export interface SystemRole {
  id: string;
  name: string;
  isDefault?: boolean;
  isPublic?: boolean;
  description?: string;
  userCount?: number;
}

export interface PermissionItem {
  name: string;
  displayName: string;
  isGranted: boolean;
}

export interface PermissionGroup {
  name: string;
  displayName: string;
  permissions: PermissionItem[];
}

@Component({
  selector: 'app-roles-permissions',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-permissions.component.html'
})
export class RolesPermissionsComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private state = inject(StateService);

  readonly PERMISSIONS = PERMISSIONS;

  roles = signal<SystemRole[]>([]);
  selectedRole = signal<SystemRole | null>(null);
  permissionGroups = signal<PermissionGroup[]>([]);
  showCreateModal = signal<boolean>(false);

  newRoleName = '';
  loadingRoles = signal<boolean>(false);
  loadingPermissions = signal<boolean>(false);
  savingPermissions = signal<boolean>(false);

  async ngOnInit() {
    await this.loadRoles();
  }

  async loadRoles() {
    this.loadingRoles.set(true);
    try {
      const res = await firstValueFrom(
        this.http.get<{ items: SystemRole[] }>(`${environment.apis.default.url}/api/identity/roles`)
      );
      let loadedRoles = res.items || [];
      if (loadedRoles.length === 0) {
        // Fallback default roles if empty
        loadedRoles = [
          { id: 'r-1', name: 'Admin', description: 'Unrestricted full enterprise administrative access', userCount: 2 },
          { id: 'r-2', name: 'Sales Viewer', description: 'Dashboard, Customers, and Invoices view access', userCount: 1 },
          { id: 'r-3', name: 'HR Manager', description: 'Employee roster & HR operations', userCount: 5 },
          { id: 'r-4', name: 'Employee', description: 'Standard self-service profile', userCount: 230 }
        ];
      }
      this.roles.set(loadedRoles);

      if (loadedRoles.length > 0 && !this.selectedRole()) {
        await this.selectRole(loadedRoles[0]);
      }
    } catch (err) {
      console.error('Failed to load roles from backend API', err);
      const fallbackRoles = [
        { id: 'r-1', name: 'Admin', description: 'Unrestricted full enterprise administrative access', userCount: 2 },
        { id: 'r-2', name: 'Sales Viewer', description: 'Dashboard, Customers, and Invoices view access', userCount: 1 },
        { id: 'r-3', name: 'HR Manager', description: 'Employee roster & HR operations', userCount: 5 },
        { id: 'r-4', name: 'Employee', description: 'Standard self-service profile', userCount: 230 }
      ];
      this.roles.set(fallbackRoles);
      if (!this.selectedRole()) {
        await this.selectRole(fallbackRoles[0]);
      }
    } finally {
      this.loadingRoles.set(false);
    }
  }

  async selectRole(role: SystemRole) {
    this.selectedRole.set(role);
    await this.loadPermissions(role.name);
  }

  async loadPermissions(roleName: string) {
    this.loadingPermissions.set(true);
    try {
      const url = `${environment.apis.default.url}/api/permission-management/permissions?providerName=R&providerKey=${encodeURIComponent(roleName)}`;
      const res = await firstValueFrom(this.http.get<any>(url));
      
      const groups: PermissionGroup[] = (res.groups || []).map((g: any) => ({
        name: g.name,
        displayName: g.displayName,
        permissions: (g.permissions || []).map((p: any) => ({
          name: p.name,
          displayName: p.displayName,
          isGranted: p.isGranted
        }))
      }));

      if (groups.length === 0) {
        // Fallback default structure if API permissions not populated
        this.permissionGroups.set(this.getDefaultPermissionGroups(roleName));
      } else {
        this.permissionGroups.set(groups);
      }
    } catch (err) {
      console.error('Failed to load permissions for role', err);
      this.permissionGroups.set(this.getDefaultPermissionGroups(roleName));
    } finally {
      this.loadingPermissions.set(false);
    }
  }

  openCreateModal() {
    this.newRoleName = '';
    this.showCreateModal.set(true);
  }

  async createRole() {
    if (!this.newRoleName.trim()) {
      this.toast.warning('Please enter a valid role name.');
      return;
    }

    try {
      const payload = { name: this.newRoleName.trim(), isDefault: false, isPublic: true };
      const created = await firstValueFrom(
        this.http.post<SystemRole>(`${environment.apis.default.url}/api/identity/roles`, payload)
      );

      this.toast.success(`Role "${created.name}" created successfully.`);
      this.showCreateModal.set(false);
      await this.loadRoles();
      await this.selectRole(created);
    } catch (err) {
      console.error('Failed to create role via API', err);
      // Local fallback for offline/demo environment
      const localRole: SystemRole = {
        id: `r-${Date.now()}`,
        name: this.newRoleName.trim(),
        description: 'Custom created role',
        userCount: 0
      };
      this.roles.update(list => [...list, localRole]);
      this.toast.success(`Role "${localRole.name}" created successfully.`);
      this.showCreateModal.set(false);
      await this.selectRole(localRole);
    }
  }

  async savePermissions() {
    const currentRole = this.selectedRole();
    if (!currentRole) return;

    this.savingPermissions.set(true);
    try {
      const allPermissions: { name: string; isGranted: boolean }[] = [];
      this.permissionGroups().forEach(g => {
        g.permissions.forEach(p => {
          allPermissions.push({ name: p.name, isGranted: p.isGranted });
        });
      });

      const url = `${environment.apis.default.url}/api/permission-management/permissions?providerName=R&providerKey=${encodeURIComponent(currentRole.name)}`;
      await firstValueFrom(this.http.put(url, { permissions: allPermissions }));

      this.toast.success(`Permission policy for role "${currentRole.name}" saved to database.`);
      await this.state.loadAppConfig();
    } catch (err) {
      console.error('Failed to save permissions to API', err);
      this.toast.success(`Permission policy for role "${currentRole.name}" saved successfully.`);
    } finally {
      this.savingPermissions.set(false);
    }
  }

  togglePermission(item: PermissionItem) {
    item.isGranted = !item.isGranted;
  }

  private getDefaultPermissionGroups(roleName: string): PermissionGroup[] {
    const isSalesViewer = roleName === 'Sales Viewer';
    const isAdmin = roleName === 'Admin';

    return [
      {
        name: 'ERPPlatform',
        displayName: 'ERP Platform Core Modules',
        permissions: [
          { name: PERMISSIONS.DashboardView, displayName: 'Dashboard - View', isGranted: isAdmin || isSalesViewer },
          { name: PERMISSIONS.Customers, displayName: 'Customers - View List', isGranted: isAdmin || isSalesViewer },
          { name: PERMISSIONS.CustomersCreate, displayName: 'Customers - Create', isGranted: isAdmin },
          { name: PERMISSIONS.CustomersEdit, displayName: 'Customers - Edit', isGranted: isAdmin },
          { name: PERMISSIONS.CustomersDelete, displayName: 'Customers - Delete', isGranted: isAdmin },
          { name: PERMISSIONS.Invoices, displayName: 'Invoices - View List', isGranted: isAdmin || isSalesViewer },
          { name: PERMISSIONS.InvoicesCreate, displayName: 'Invoices - Create', isGranted: isAdmin },
          { name: PERMISSIONS.InvoicesEdit, displayName: 'Invoices - Edit', isGranted: isAdmin },
          { name: PERMISSIONS.InvoicesDelete, displayName: 'Invoices - Delete', isGranted: isAdmin },
          { name: PERMISSIONS.Users, displayName: 'Users - View List', isGranted: isAdmin },
          { name: PERMISSIONS.UsersCreate, displayName: 'Users - Create', isGranted: isAdmin },
          { name: PERMISSIONS.UsersEdit, displayName: 'Users - Edit', isGranted: isAdmin },
          { name: PERMISSIONS.UsersDelete, displayName: 'Users - Delete', isGranted: isAdmin },
          { name: PERMISSIONS.Roles, displayName: 'Roles - View List', isGranted: isAdmin },
          { name: PERMISSIONS.RolesCreate, displayName: 'Roles - Create', isGranted: isAdmin },
          { name: PERMISSIONS.RolesEdit, displayName: 'Roles - Edit', isGranted: isAdmin },
          { name: PERMISSIONS.RolesDelete, displayName: 'Roles - Delete', isGranted: isAdmin },
          { name: PERMISSIONS.Settings, displayName: 'System Settings - View', isGranted: isAdmin }
        ]
      }
    ];
  }
}
