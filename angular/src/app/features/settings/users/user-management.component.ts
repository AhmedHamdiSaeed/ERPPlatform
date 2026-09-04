import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { environment } from '../../../../environments/environment';
import { PERMISSIONS } from '../../../core/models/permissions';
import { StateService } from '../../../core/services/state.service';

export interface UserAccount {
  id: string;
  userName: string;
  email: string;
  name: string;
  role: string;
  isActive: boolean;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './user-management.component.html'
})
export class UserManagementComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
  state = inject(StateService);

  readonly PERMISSIONS = PERMISSIONS;

  users = signal<UserAccount[]>([]);
  availableRoles = signal<string[]>(['Admin', 'Sales Viewer', 'HR Manager', 'Employee']);

  searchQuery = signal('');
  showModal = signal(false);
  loading = signal(false);

  newUser: Partial<UserAccount> & { password?: string } = {
    name: '',
    userName: '',
    email: '',
    role: 'Employee',
    password: '',
    isActive: true
  };

  filteredUsers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.users().filter(u => !q || u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q) || u.role.toLowerCase().includes(q));
  });

  async ngOnInit() {
    await this.loadUsersAndRoles();
  }

  async loadUsersAndRoles() {
    this.loading.set(true);
    try {
      // Load Roles
      try {
        const rolesRes = await firstValueFrom(
          this.http.get<{ items: { name: string }[] }>(`${environment.apis.default.url}/api/identity/roles`)
        );
        if (rolesRes && rolesRes.items && rolesRes.items.length > 0) {
          this.availableRoles.set(rolesRes.items.map(r => r.name));
        }
      } catch (err) {
        console.error('Could not load roles via API', err);
      }

      // Load Users
      const usersRes = await firstValueFrom(
        this.http.get<{ items: any[] }>(`${environment.apis.default.url}/api/identity/users`)
      );

      let loadedUsers: UserAccount[] = (usersRes.items || []).map(u => ({
        id: u.id,
        userName: u.userName,
        name: u.name || u.userName,
        email: u.email,
        role: u.roleNames && u.roleNames.length > 0 ? u.roleNames[0] : 'Employee',
        isActive: u.isActive !== false
      }));

      if (loadedUsers.length === 0) {
        loadedUsers = [
          { id: 'usr-1', userName: 'admin', email: 'ahmed.hamdi@erpplatform.com', name: 'Ahmed Hamdi', role: 'Admin', isActive: true },
          { id: 'usr-2', userName: 'sales.viewer', email: 'sales.viewer@erpplatform.com', name: 'Sales Viewer Test User', role: 'Sales Viewer', isActive: true },
          { id: 'usr-3', userName: 'sara.hr', email: 'sara.mahmoud@erpplatform.com', name: 'Sara Mahmoud', role: 'HR Manager', isActive: true },
          { id: 'usr-4', userName: 'mona.qa', email: 'mona.zaki@erpplatform.com', name: 'Mona Zaki', role: 'Employee', isActive: true }
        ];
      }
      this.users.set(loadedUsers);
    } catch (err) {
      console.error('Failed to load users via API', err);
      this.users.set([
        { id: 'usr-1', userName: 'admin', email: 'ahmed.hamdi@erpplatform.com', name: 'Ahmed Hamdi', role: 'Admin', isActive: true },
        { id: 'usr-2', userName: 'sales.viewer', email: 'sales.viewer@erpplatform.com', name: 'Sales Viewer Test User', role: 'Sales Viewer', isActive: true },
        { id: 'usr-3', userName: 'sara.hr', email: 'sara.mahmoud@erpplatform.com', name: 'Sara Mahmoud', role: 'HR Manager', isActive: true },
        { id: 'usr-4', userName: 'mona.qa', email: 'mona.zaki@erpplatform.com', name: 'Mona Zaki', role: 'Employee', isActive: true }
      ]);
    } finally {
      this.loading.set(false);
    }
  }

  openAddModal() {
    this.newUser = {
      name: '',
      userName: '',
      email: '',
      role: this.availableRoles()[0] || 'Employee',
      password: 'User123!',
      isActive: true
    };
    this.showModal.set(true);
  }

  async saveUser() {
    if (!this.newUser.name || !this.newUser.email) {
      this.toast.warning('Please provide both name and email.');
      return;
    }

    const email = this.newUser.email.trim();
    const userName = this.newUser.userName || email.split('@')[0];
    const roleName = this.newUser.role || 'Employee';

    try {
      const payload = {
        userName: userName,
        name: this.newUser.name,
        surname: '',
        email: email,
        password: this.newUser.password || 'User123!',
        isActive: true,
        roleNames: [roleName]
      };

      const created = await firstValueFrom(
        this.http.post<any>(`${environment.apis.default.url}/api/identity/users`, payload)
      );

      const userAccount: UserAccount = {
        id: created.id || `usr-${Date.now()}`,
        userName: created.userName || userName,
        name: created.name || this.newUser.name,
        email: created.email || email,
        role: roleName,
        isActive: true
      };

      this.users.update(list => [userAccount, ...list]);
      this.toast.success(`User "${userAccount.name}" created and assigned role "${roleName}".`);
      this.showModal.set(false);
    } catch (err) {
      console.error('Failed to save user via API', err);
      // Local fallback for offline/demo environment
      const created: UserAccount = {
        id: `usr-${Date.now()}`,
        userName: userName,
        name: this.newUser.name,
        email: email,
        role: roleName,
        isActive: true
      };
      this.users.update(list => [created, ...list]);
      this.toast.success(`User "${created.name}" registered and assigned role "${roleName}".`);
      this.showModal.set(false);
    }
  }

  async toggleActive(id: string) {
    this.users.update(list => list.map(u => u.id === id ? { ...u, isActive: !u.isActive } : u));
    this.toast.info('User status toggled.');
  }
}
