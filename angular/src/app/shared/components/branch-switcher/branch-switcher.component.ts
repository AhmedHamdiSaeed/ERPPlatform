import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrgApiService, Branch } from '../../../core/services/api/org-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-branch-switcher',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './branch-switcher.component.html'
})
export class BranchSwitcherComponent {
  private orgApi = inject(OrgApiService);
  private toast = inject(ToastService);

  branches = signal<Branch[]>([]);
  activeBranch = signal<Branch | null>(null);
  isOpen = signal(false);

  constructor() {
    this.loadBranches();
  }

  async loadBranches() {
    try {
      const list = await this.orgApi.getBranches();
      this.branches.set(list);
      if (list.length > 0) {
        const savedBranchId = localStorage.getItem('erp_active_branch_id');
        const active = list.find(b => b.id === savedBranchId) || list[0];
        this.activeBranch.set(active);
      }
    } catch (e) {
      console.error('Failed to load branches for switcher', e);
    }
  }

  selectBranch(branch: Branch) {
    this.activeBranch.set(branch);
    localStorage.setItem('erp_active_branch_id', branch.id);
    localStorage.setItem('erp_active_company_id', branch.companyId);
    this.isOpen.set(false);
    this.toast.success(`Active branch context switched to: ${branch.name}`);
  }

  toggleDropdown() {
    this.isOpen.update(v => !v);
  }
}
