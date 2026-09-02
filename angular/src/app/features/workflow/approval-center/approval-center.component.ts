import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ApprovalCenterApiService,
  ApprovalEntityType,
  PendingApproval
} from '../../../core/services/api/approval-center-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-approval-center',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './approval-center.component.html'
})
export class ApprovalCenterComponent {
  private approvalApi = inject(ApprovalCenterApiService);
  private toast = inject(ToastService);

  approvals = signal<PendingApproval[]>([]);
  selectedIds = signal<Set<string>>(new Set());
  loading = signal(false);
  approving = signal(false);
  loadError = signal<string | null>(null);

  typeFilter = signal<'ALL' | ApprovalEntityType>('ALL');
  comment = '';

  entityTypes: ApprovalEntityType[] = [
    'LeaveRequest',
    'ExpenseRequest',
    'SalesOrder',
    'PurchaseRequest',
    'WorkflowTask'
  ];

  filtered = computed(() => {
    const filter = this.typeFilter();
    if (filter === 'ALL') return this.approvals();
    return this.approvals().filter(a => a.entityType === filter);
  });

  selectedCount = computed(() => this.selectedIds().size);

  constructor() {
    this.load();
  }

  async load() {
    this.loading.set(true);
    this.loadError.set(null);
    this.selectedIds.set(new Set());
    try {
      this.approvals.set(await this.approvalApi.getPendingApprovals());
    } catch (e) {
      console.error('Failed to load pending approvals', e);
      this.loadError.set('Could not load pending approvals.');
      this.toast.error('Could not load pending approvals from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  isSelected(id: string): boolean {
    return this.selectedIds().has(id);
  }

  toggleSelection(id: string) {
    const next = new Set(this.selectedIds());
    if (next.has(id)) next.delete(id);
    else next.add(id);
    this.selectedIds.set(next);
  }

  toggleSelectAllCurrent() {
    const current = this.filtered();
    const next = new Set(this.selectedIds());
    const allSelected = current.length > 0 && current.every(a => next.has(a.id));

    if (allSelected) {
      current.forEach(a => next.delete(a.id));
    } else {
      current.forEach(a => next.add(a.id));
    }

    this.selectedIds.set(next);
  }

  allCurrentSelected(): boolean {
    const current = this.filtered();
    return current.length > 0 && current.every(a => this.selectedIds().has(a.id));
  }

  async approveOne(item: PendingApproval) {
    await this.approveByType(item.entityType, [item.id]);
  }

  async approveSelected() {
    const ids = [...this.selectedIds()];
    if (ids.length === 0) {
      this.toast.warning('Select at least one approval item first.');
      return;
    }

    const grouped = new Map<ApprovalEntityType, string[]>();
    for (const item of this.approvals()) {
      if (!this.selectedIds().has(item.id)) continue;
      if (!grouped.has(item.entityType)) grouped.set(item.entityType, []);
      grouped.get(item.entityType)!.push(item.id);
    }

    this.approving.set(true);
    try {
      for (const [type, typeIds] of grouped) {
        await this.approvalApi.batchApprove({
          entityType: type,
          ids: typeIds,
          comments: this.comment
        });
      }
      this.toast.success(`Approved ${ids.length} item(s).`);
      this.comment = '';
      await this.load();
    } catch (e) {
      console.error('Failed batch approve', e);
      this.toast.error('Could not complete batch approval.');
    } finally {
      this.approving.set(false);
    }
  }

  private async approveByType(type: ApprovalEntityType, ids: string[]) {
    this.approving.set(true);
    try {
      await this.approvalApi.batchApprove({ entityType: type, ids, comments: this.comment });
      this.toast.success('Approval completed.');
      await this.load();
    } catch (e) {
      console.error('Failed to approve item', e);
      this.toast.error('Could not approve this item.');
    } finally {
      this.approving.set(false);
    }
  }
}
