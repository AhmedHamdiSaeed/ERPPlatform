import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CrmApiService, CrmActivity } from '../../../core/services/api/crm-api.service';
import { CustomerApiService, CustomerProfile } from '../../../core/services/api/customer-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

/**
 * CRM activities — calls, meetings, tasks, emails and follow-ups.
 * Backed by /api/app/crm-activity (CRUD + complete / reopen).
 */
@Component({
  selector: 'app-crm-activities',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './activities.component.html'
})
export class ActivitiesComponent {
  private crmApi = inject(CrmApiService);
  private customerApi = inject(CustomerApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  readonly types = ['Call', 'Meeting', 'Task', 'Email', 'FollowUp'];
  readonly statuses = ['Open', 'Completed', 'Cancelled'];
  readonly priorities = ['Low', 'Normal', 'High'];

  activities = signal<CrmActivity[]>([]);
  customers = signal<CustomerProfile[]>([]);

  typeFilter = signal('');
  statusFilter = signal('');
  overdueOnly = signal(false);
  search = signal('');
  showModal = signal(false);

  draft: Partial<CrmActivity> = this.emptyDraft();

  constructor() {
    this.load();
  }

  private emptyDraft(): Partial<CrmActivity> {
    return {
      type: 'Task',
      subject: '',
      description: '',
      priority: 'Normal',
      status: 'Open',
      dueDate: this.tomorrow(),
      assignedTo: '',
      customerId: null
    };
  }

  private tomorrow(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return d.toISOString().split('T')[0];
  }

  async load(): Promise<void> {
    const [activities, customers] = await Promise.all([
      this.crmApi.getActivities({
        type: this.typeFilter() || undefined,
        status: this.statusFilter() || undefined,
        onlyOverdue: this.overdueOnly() || undefined
      }),
      this.customerApi.getCustomers().catch(() => [] as CustomerProfile[])
    ]);
    this.activities.set(activities);
    this.customers.set(customers);
  }

  async setType(value: string): Promise<void> {
    this.typeFilter.set(value);
    await this.load();
  }

  async setStatus(value: string): Promise<void> {
    this.statusFilter.set(value);
    await this.load();
  }

  async toggleOverdue(): Promise<void> {
    this.overdueOnly.update(v => !v);
    await this.load();
  }

  /** Client-side text filter — the server filter is reserved for the typed fields. */
  filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    const list = this.activities();
    if (!term) return list;
    return list.filter(a =>
      (a.subject || '').toLowerCase().includes(term) ||
      (a.description || '').toLowerCase().includes(term) ||
      (a.assignedTo || '').toLowerCase().includes(term) ||
      (a.customerName || '').toLowerCase().includes(term)
    );
  });

  openCount = computed(() => this.activities().filter(a => a.status === 'Open').length);
  overdueCount = computed(() => this.activities().filter(a => a.isOverdue).length);

  openAdd(): void {
    this.draft = this.emptyDraft();
    this.showModal.set(true);
  }

  async save(): Promise<void> {
    if (!this.draft.subject?.trim()) {
      this.toast.warning('An activity needs a subject.');
      return;
    }
    await this.crmApi.createActivity(this.draft);
    this.toast.success('Activity created.');
    this.showModal.set(false);
    await this.load();
  }

  async complete(a: CrmActivity): Promise<void> {
    const outcome = await this.dialog.prompt({
      title: 'Complete Activity',
      message: `Outcome for "${a.subject}"?`,
      placeholder: 'Outcome / Notes',
      confirmText: 'Complete',
      type: 'success',
      icon: 'pi-check-circle'
    });
    if (outcome === null) return;
    await this.crmApi.completeActivity(a.id, outcome);
    this.toast.success('Activity completed.');
    await this.load();
  }

  async reopen(a: CrmActivity): Promise<void> {
    await this.crmApi.reopenActivity(a.id);
    this.toast.success('Activity reopened.');
    await this.load();
  }

  async remove(a: CrmActivity): Promise<void> {
    const confirmed = await this.dialog.confirm({
      title: 'Delete Activity',
      message: `Are you sure you want to delete "${a.subject}"?`,
      confirmText: 'Delete',
      type: 'danger'
    });
    if (!confirmed) return;

    await this.crmApi.deleteActivity(a.id);
    this.toast.success('Activity removed.');
    await this.load();
  }

  iconFor(type: string): string {
    switch (type) {
      case 'Call': return 'pi-phone';
      case 'Meeting': return 'pi-users';
      case 'Task': return 'pi-check-square';
      case 'Email': return 'pi-envelope';
      case 'FollowUp': return 'pi-clock';
      default: return 'pi-circle';
    }
  }

  dueLabel(a: CrmActivity): string {
    const due = new Date(a.dueDate).getTime();
    if (isNaN(due)) return '';
    const days = Math.round((due - Date.now()) / 86400000);
    if (days === 0) return 'Today';
    if (days === 1) return 'Tomorrow';
    if (days === -1) return 'Yesterday';
    return days > 0 ? `in ${days} days` : `${Math.abs(days)} days overdue`;
  }

  statusClass(a: CrmActivity): string {
    if (a.status === 'Completed') return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';
    if (a.isOverdue) return 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300';
    if (a.status === 'Cancelled') return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
    return 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300';
  }

  priorityClass(priority: string): string {
    switch (priority) {
      case 'High': return 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300';
      case 'Low': return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
      default: return 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300';
    }
  }
}
