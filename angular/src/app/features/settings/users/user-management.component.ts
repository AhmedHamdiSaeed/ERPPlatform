import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { environment } from '../../../../environments/environment';

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
  imports: [FormsModule],
  templateUrl: './user-management.component.html'
})
export class UserManagementComponent {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  users = signal<UserAccount[]>([
    { id: 'usr-1', userName: 'admin', email: 'ahmed.hamdi@erpplatform.com', name: 'Ahmed Hamdi', role: 'Admin', isActive: true },
    { id: 'usr-2', userName: 'sara.hr', email: 'sara.mahmoud@erpplatform.com', name: 'Sara Mahmoud', role: 'HR Manager', isActive: true },
    { id: 'usr-3', userName: 'omar.logistics', email: 'omar.farouk@erpplatform.com', name: 'Omar Farouk', role: 'Inventory Manager', isActive: true },
    { id: 'usr-4', userName: 'mona.qa', email: 'mona.zaki@erpplatform.com', name: 'Mona Zaki', role: 'Employee', isActive: true }
  ]);

  searchQuery = signal('');
  showModal = signal(false);

  newUser: Partial<UserAccount> = { name: '', userName: '', email: '', role: 'Employee', isActive: true };

  filteredUsers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.users().filter(u => !q || u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q) || u.role.toLowerCase().includes(q));
  });

  openAddModal() {
    this.newUser = { role: 'Employee', isActive: true };
    this.showModal.set(true);
  }

  async saveUser() {
    if (!this.newUser.name || !this.newUser.email) {
      this.toast.warning('Please provide both name and email.');
      return;
    }
    const created: UserAccount = {
      id: `usr-${Date.now()}`,
      userName: this.newUser.userName || this.newUser.email.split('@')[0],
      name: this.newUser.name,
      email: this.newUser.email,
      role: this.newUser.role || 'Employee',
      isActive: true
    };
    this.users.update(list => [created, ...list]);
    this.toast.success('User account registered.');
    this.showModal.set(false);
  }

  async toggleActive(id: string) {
    this.users.update(list => list.map(u => u.id === id ? { ...u, isActive: !u.isActive } : u));
    this.toast.info('User status toggled.');
  }
}
