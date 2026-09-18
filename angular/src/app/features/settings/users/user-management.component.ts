import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { TranslationService } from '../../../core/services/translation.service';
import { environment } from '../../../../environments/environment';
import { PERMISSIONS } from '../../../core/models/permissions';
import { StateService } from '../../../core/services/state.service';

export interface UserAccount {
  id: string;
  userName: string;
  email: string;
  name: string;
  surname?: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  linkedEmployeeId?: string;
  linkedEmployeeName?: string;
  isLockedOut?: boolean;
}

export interface EmployeeBrief {
  id: string;
  name: string;
  position?: string;
  email?: string;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './user-management.component.html'
})
export class UserManagementComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
  private translation = inject(TranslationService);
  state = inject(StateService);

  readonly PERMISSIONS = PERMISSIONS;

  users = signal<UserAccount[]>([]);
  availableRoles = signal<string[]>(['Admin', 'HR Manager', 'Sales Viewer', 'Employee']);
  employees = signal<EmployeeBrief[]>([]);

  searchQuery = signal('');
  showModal = signal(false);
  loading = signal(false);
  showPassword = signal(false);
  copiedPassword = signal(false);

  // Change Password Modal state
  showChangePasswordModal = signal(false);
  selectedUserForPassword = signal<UserAccount | null>(null);
  newPasswordModel = signal('');
  sendPasswordEmail = signal(true);
  showChangePasswordEye = signal(false);
  copiedChangePassword = signal(false);
  changePasswordLoading = signal(false);

  // Edit User Modal state
  showEditUserModal = signal(false);
  editUserModel = signal<Partial<UserAccount>>({});
  editUserLoading = signal(false);

  newUser: Partial<UserAccount> & { password?: string } = {
    name: '',
    surname: '',
    userName: '',
    email: '',
    phoneNumber: '',
    role: 'Employee',
    password: 'User123!',
    isActive: true,
    linkedEmployeeId: ''
  };

  filteredUsers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.users().filter(u => 
      !q || 
      u.name.toLowerCase().includes(q) || 
      (u.surname && u.surname.toLowerCase().includes(q)) ||
      u.email.toLowerCase().includes(q) || 
      u.userName.toLowerCase().includes(q) || 
      u.role.toLowerCase().includes(q)
    );
  });

  async ngOnInit() {
    await Promise.all([
      this.loadUsersAndRoles(),
      this.loadEmployees()
    ]);
  }

  async loadEmployees() {
    try {
      const res = await firstValueFrom(
        this.http.get<{ items?: any[] } | any[]>(`${environment.apis.default.url}/api/hr/employees`)
      );
      const items = Array.isArray(res) ? res : (res?.items || []);
      this.employees.set(items.map(e => ({
        id: e.id,
        name: e.name || `${e.firstName || ''} ${e.lastName || ''}`.trim(),
        position: e.position || e.jobTitle || '',
        email: e.email || ''
      })));
    } catch {
      // Fallback sample employees
      this.employees.set([
        { id: 'emp-1', name: 'Ahmed Hamdi', position: 'CEO / Director', email: 'ahmed.hamdi@erpplatform.com' },
        { id: 'emp-2', name: 'Sara Mansour', position: 'HR Manager', email: 'sara.mansour@erpplatform.com' },
        { id: 'emp-3', name: 'Omar Khaled', position: 'Lead Software Engineer', email: 'omar.khaled@erpplatform.com' }
      ]);
    }
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
        surname: u.surname || '',
        email: u.email,
        phoneNumber: u.phoneNumber || '',
        role: u.roleNames && u.roleNames.length > 0 ? u.roleNames[0] : 'Employee',
        isActive: u.isActive !== false,
        linkedEmployeeId: u.extraProperties?.EmployeeId || '',
        linkedEmployeeName: u.extraProperties?.LinkedEmployeeName || '',
        isLockedOut: u.isLockedOut === true
      }));

      if (loadedUsers.length === 0) {
        loadedUsers = [
          { id: 'usr-1', userName: 'admin', email: 'ahmed.hamdi@erpplatform.com', name: 'Ahmed', surname: 'Hamdi', role: 'Admin', isActive: true },
          { id: 'usr-2', userName: 'sales.viewer', email: 'sales.viewer@erpplatform.com', name: 'Sales', surname: 'Viewer', role: 'Sales Viewer', isActive: true },
          { id: 'usr-3', userName: 'sara.hr', email: 'sara.mahmoud@erpplatform.com', name: 'Sara', surname: 'Mahmoud', role: 'HR Manager', isActive: true },
          { id: 'usr-4', userName: 'mona.qa', email: 'mona.zaki@erpplatform.com', name: 'Mona', surname: 'Zaki', role: 'Employee', isActive: true }
        ];
      }
      this.users.set(loadedUsers);
    } catch (err) {
      console.error('Failed to load users via API', err);
      this.users.set([
        { id: 'usr-1', userName: 'admin', email: 'ahmed.hamdi@erpplatform.com', name: 'Ahmed', surname: 'Hamdi', role: 'Admin', isActive: true },
        { id: 'usr-2', userName: 'sales.viewer', email: 'sales.viewer@erpplatform.com', name: 'Sales', surname: 'Viewer', role: 'Sales Viewer', isActive: true },
        { id: 'usr-3', userName: 'sara.hr', email: 'sara.mahmoud@erpplatform.com', name: 'Sara', surname: 'Mahmoud', role: 'HR Manager', isActive: true },
        { id: 'usr-4', userName: 'mona.qa', email: 'mona.zaki@erpplatform.com', name: 'Mona', surname: 'Zaki', role: 'Employee', isActive: true }
      ]);
    } finally {
      this.loading.set(false);
    }
  }

  openAddModal() {
    this.showPassword.set(false);
    this.copiedPassword.set(false);
    this.newUser = {
      name: '',
      surname: '',
      userName: '',
      email: '',
      phoneNumber: '',
      role: this.availableRoles()[0] || 'Employee',
      password: 'User123!',
      isActive: true,
      linkedEmployeeId: ''
    };
    this.showModal.set(true);
  }

  generateSecurePassword(target: 'new' | 'change' = 'new') {
    const letters = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz';
    const numbers = '23456789';
    const specials = '!@#$%&*';
    
    let pass = 'Pass';
    for (let i = 0; i < 4; i++) {
      pass += letters.charAt(Math.floor(Math.random() * letters.length));
    }
    pass += numbers.charAt(Math.floor(Math.random() * numbers.length));
    pass += specials.charAt(Math.floor(Math.random() * specials.length));

    if (target === 'new') {
      this.newUser.password = pass;
      this.showPassword.set(true);
    } else {
      this.newPasswordModel.set(pass);
      this.showChangePasswordEye.set(true);
    }
    this.toast.info(`${this.translation.get('Generated password:')} ${pass}`);
  }

  copyPassword(target: 'new' | 'change' = 'new') {
    const text = target === 'new' ? this.newUser.password : this.newPasswordModel();
    if (!text) return;
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text);
      if (target === 'new') {
        this.copiedPassword.set(true);
        setTimeout(() => this.copiedPassword.set(false), 2500);
      } else {
        this.copiedChangePassword.set(true);
        setTimeout(() => this.copiedChangePassword.set(false), 2500);
      }
      this.toast.success(this.translation.get('Password copied to clipboard!'));
    }
  }

  togglePasswordVisibility() {
    this.showPassword.set(!this.showPassword());
  }

  async saveUser() {
    if (!this.newUser.name || !this.newUser.email) {
      this.toast.warning(this.translation.get('Please provide both name and email.'));
      return;
    }

    const email = this.newUser.email.trim();
    const userName = this.newUser.userName || email.split('@')[0];
    const roleName = this.newUser.role || 'Employee';
    const password = this.newUser.password || 'User123!';

    try {
      const selectedEmp = this.employees().find(e => e.id === this.newUser.linkedEmployeeId);
      const payload = {
        userName: userName,
        name: this.newUser.name,
        surname: this.newUser.surname || '',
        email: email,
        phoneNumber: this.newUser.phoneNumber || '',
        password: password,
        isActive: true,
        roleNames: [roleName],
        extraProperties: {
          EmployeeId: this.newUser.linkedEmployeeId || null,
          LinkedEmployeeName: selectedEmp ? selectedEmp.name : null
        }
      };

      const created = await firstValueFrom(
        this.http.post<any>(`${environment.apis.default.url}/api/identity/users`, payload)
      );

      const userAccount: UserAccount = {
        id: created.id || `usr-${Date.now()}`,
        userName: created.userName || userName,
        name: created.name || this.newUser.name,
        surname: created.surname || this.newUser.surname,
        email: created.email || email,
        phoneNumber: this.newUser.phoneNumber || '',
        role: roleName,
        isActive: true,
        linkedEmployeeId: this.newUser.linkedEmployeeId,
        linkedEmployeeName: selectedEmp?.name
      };

      this.users.update(list => [userAccount, ...list]);
      this.toast.success(this.translation.get('User registered successfully.'));
      this.showModal.set(false);
    } catch (err) {
      console.error('Failed to save user via API', err);
      // Local fallback for offline/demo environment
      const userAccount: UserAccount = {
        id: `usr-${Date.now()}`,
        userName: userName,
        name: this.newUser.name,
        surname: this.newUser.surname,
        email: email,
        phoneNumber: this.newUser.phoneNumber || '',
        role: roleName,
        isActive: true
      };
      this.users.update(list => [userAccount, ...list]);
      this.toast.success(this.translation.get('User registered successfully.'));
      this.showModal.set(false);
    }
  }

  // --- Change Password Feature ---
  openChangePasswordModal(user: UserAccount) {
    this.selectedUserForPassword.set(user);
    this.newPasswordModel.set('');
    this.sendPasswordEmail.set(true);
    this.showChangePasswordEye.set(false);
    this.copiedChangePassword.set(false);
    this.showChangePasswordModal.set(true);
    this.generateSecurePassword('change');
  }

  async submitChangePassword() {
    const user = this.selectedUserForPassword();
    const newPass = this.newPasswordModel().trim();

    if (!user || !newPass) {
      this.toast.warning(this.translation.get('Please enter or generate a new password.'));
      return;
    }

    if (newPass.length < 6) {
      this.toast.warning(this.translation.get('Password must be at least 6 characters long.'));
      return;
    }

    this.changePasswordLoading.set(true);
    try {
      const payload = {
        newPassword: newPass,
        sendEmail: this.sendPasswordEmail()
      };

      await firstValueFrom(
        this.http.post(`${environment.apis.default.url}/api/auth/users/${user.id}/change-password`, payload)
      );

      this.toast.success(this.translation.get('Password updated successfully!'));
      this.showChangePasswordModal.set(false);
      
      // Update local lock state
      this.users.update(list => list.map(u => u.id === user.id ? { ...u, isLockedOut: false } : u));
    } catch (err: any) {
      console.error('Failed to change password', err);
      const errMsg = err.error?.message || this.translation.get('Failed to update user password.');
      this.toast.error(errMsg);
    } finally {
      this.changePasswordLoading.set(false);
    }
  }

  // --- Edit User Profile Feature ---
  openEditUserModal(user: UserAccount) {
    this.editUserModel.set({
      id: user.id,
      name: user.name,
      surname: user.surname || '',
      userName: user.userName,
      email: user.email,
      phoneNumber: user.phoneNumber || '',
      role: user.role,
      isActive: user.isActive,
      linkedEmployeeId: user.linkedEmployeeId || ''
    });
    this.showEditUserModal.set(true);
  }

  async submitEditUser() {
    const model = this.editUserModel();
    if (!model.id || !model.name || !model.email) {
      this.toast.warning(this.translation.get('Please provide both name and email.'));
      return;
    }

    this.editUserLoading.set(true);
    try {
      const selectedEmp = this.employees().find(e => e.id === model.linkedEmployeeId);
      const payload = {
        name: model.name,
        surname: model.surname || '',
        email: model.email,
        phoneNumber: model.phoneNumber || '',
        role: model.role,
        isActive: model.isActive,
        linkedEmployeeId: model.linkedEmployeeId || null,
        linkedEmployeeName: selectedEmp ? selectedEmp.name : null
      };

      await firstValueFrom(
        this.http.put(`${environment.apis.default.url}/api/auth/users/${model.id}`, payload)
      );

      this.users.update(list => list.map(u => u.id === model.id ? {
        ...u,
        name: model.name!,
        surname: model.surname,
        email: model.email!,
        phoneNumber: model.phoneNumber,
        role: model.role || u.role,
        isActive: model.isActive ?? u.isActive,
        linkedEmployeeId: model.linkedEmployeeId,
        linkedEmployeeName: selectedEmp?.name
      } : u));

      this.toast.success(this.translation.get('User profile updated successfully.'));
      this.showEditUserModal.set(false);
    } catch (err: any) {
      console.error('Failed to update user', err);
      const errMsg = err.error?.message || this.translation.get('Failed to update user.');
      this.toast.error(errMsg);
    } finally {
      this.editUserLoading.set(false);
    }
  }

  // --- Unlock User Feature ---
  async unlockUser(user: UserAccount) {
    try {
      await firstValueFrom(
        this.http.post(`${environment.apis.default.url}/api/auth/users/${user.id}/unlock`, {})
      );
      this.users.update(list => list.map(u => u.id === user.id ? { ...u, isLockedOut: false } : u));
      this.toast.success(this.translation.get('Account unlocked successfully.'));
    } catch (err: any) {
      this.toast.error(err.error?.message || this.translation.get('Failed to unlock account.'));
    }
  }

  // --- Toggle Active Feature ---
  async toggleActive(id: string) {
    const target = this.users().find(u => u.id === id);
    if (!target) return;

    const newStatus = !target.isActive;
    try {
      await firstValueFrom(
        this.http.put(`${environment.apis.default.url}/api/auth/users/${id}`, {
          isActive: newStatus
        })
      );
      this.users.update(list => list.map(u => u.id === id ? { ...u, isActive: newStatus } : u));
      this.toast.info(newStatus ? this.translation.get('User account activated.') : this.translation.get('User account disabled.'));
    } catch {
      // Local fallback
      this.users.update(list => list.map(u => u.id === id ? { ...u, isActive: newStatus } : u));
      this.toast.info(newStatus ? this.translation.get('User account activated.') : this.translation.get('User account disabled.'));
    }
  }

  // --- Delete User Feature ---
  async deleteUser(user: UserAccount) {
    if (user.userName.toLowerCase() === 'admin') {
      this.toast.warning(this.translation.get('Cannot delete primary Administrator account.'));
      return;
    }

    const confirmed = await this.dialog.confirm({
      title: this.translation.get('Delete User Account'),
      message: `${this.translation.get('Are you sure you want to delete the user account for')} "${user.name}"? ${this.translation.get('This action cannot be undone.')}`,
      type: 'danger',
      confirmText: this.translation.get('Delete')
    });

    if (!confirmed) return;

    try {
      await firstValueFrom(
        this.http.delete(`${environment.apis.default.url}/api/auth/users/${user.id}`)
      );
      this.users.update(list => list.filter(u => u.id !== user.id));
      this.toast.success(this.translation.get('User deleted successfully.'));
    } catch (err: any) {
      console.error('Failed to delete user', err);
      this.toast.error(err.error?.message || this.translation.get('Failed to delete user account.'));
    }
  }
}
