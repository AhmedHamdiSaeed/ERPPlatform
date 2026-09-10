import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import {
  CrmApiService,
  Customer360,
  CrmActivity,
  CrmNote,
  TimelineItem
} from '../../../core/services/api/crm-api.service';
import { ToastService } from '../../../core/services/toast.service';

const TIMELINE_ICONS: Record<string, string> = {
  Activity: 'pi-calendar',
  Note: 'pi-file-edit',
  Opportunity: 'pi-chart-line',
  Quotation: 'pi-file',
  Order: 'pi-shopping-bag',
  Invoice: 'pi-file-pdf',
  Payment: 'pi-wallet'
};

@Component({
  selector: 'app-customer-360',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslatePipe],
  templateUrl: './customer-360.component.html'
})
export class Customer360Component {
  private crmApi = inject(CrmApiService);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  data = signal<Customer360 | null>(null);
  loading = signal(true);
  error = signal('');

  showActivityModal = signal(false);
  showNoteModal = signal(false);
  showContactModal = signal(false);

  newActivity = { type: 'Call', subject: '', dueDate: '', priority: 'Normal', assignedTo: '' };
  newNote = { content: '', isPinned: false };
  newContact = { firstName: '', lastName: '', jobTitle: '', email: '', phone: '', mobile: '' };

  readonly timelineIcons = TIMELINE_ICONS;

  /** Most recent first — the backend already sorts, this just guards against drift. */
  timeline = computed<TimelineItem[]>(() => this.data()?.timeline ?? []);

  constructor() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.load(id);
      }
    });
  }

  async load(id: string) {
    this.loading.set(true);
    this.error.set('');
    try {
      this.data.set(await this.crmApi.getCustomer360(id));
    } catch {
      this.error.set('Could not load this customer.');
    } finally {
      this.loading.set(false);
    }
  }

  // ── Activities ───────────────────────────────────────────────────

  openActivityModal() {
    this.newActivity = {
      type: 'Call',
      subject: '',
      dueDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
      priority: 'Normal',
      assignedTo: this.data()?.customer.ownerName ?? ''
    };
    this.showActivityModal.set(true);
  }

  async saveActivity() {
    const id = this.data()?.customer.id;
    if (!id) return;

    await this.crmApi.createActivity({
      type: this.newActivity.type as CrmActivity['type'],
      subject: this.newActivity.subject,
      description: '',
      dueDate: this.newActivity.dueDate,
      status: 'Open',
      priority: this.newActivity.priority as CrmActivity['priority'],
      customerId: id,
      assignedTo: this.newActivity.assignedTo
    });

    this.toast.success('Activity scheduled.');
    this.showActivityModal.set(false);
    await this.load(id);
  }

  async completeActivity(activity: CrmActivity) {
    const id = this.data()?.customer.id;
    if (!id) return;
    await this.crmApi.completeActivity(activity.id, 'Completed from customer 360.');
    this.toast.success('Activity completed.');
    await this.load(id);
  }

  // ── Notes ────────────────────────────────────────────────────────

  openNoteModal() {
    this.newNote = { content: '', isPinned: false };
    this.showNoteModal.set(true);
  }

  async saveNote() {
    const id = this.data()?.customer.id;
    if (!id) return;

    await this.crmApi.createNote({ customerId: id, ...this.newNote });
    this.toast.success('Note added.');
    this.showNoteModal.set(false);
    await this.load(id);
  }

  async removeNote(note: CrmNote) {
    const id = this.data()?.customer.id;
    if (!id) return;
    await this.crmApi.deleteNote(note.id);
    await this.load(id);
  }

  // ── Contacts ─────────────────────────────────────────────────────

  openContactModal() {
    this.newContact = { firstName: '', lastName: '', jobTitle: '', email: '', phone: '', mobile: '' };
    this.showContactModal.set(true);
  }

  async saveContact() {
    const id = this.data()?.customer.id;
    if (!id) return;

    await this.crmApi.createContact({ customerId: id, ...this.newContact });
    this.toast.success('Contact added.');
    this.showContactModal.set(false);
    await this.load(id);
  }

  // ── Helpers ──────────────────────────────────────────────────────

  money(value: number | undefined | null): string {
    return (value ?? 0).toLocaleString('en-US', { maximumFractionDigits: 0 });
  }

  date(value: string | undefined | null): string {
    if (!value) return '—';
    const d = new Date(value);
    return isNaN(d.getTime()) ? '—' : d.toLocaleDateString();
  }

  initials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(p => p[0]?.toUpperCase() ?? '')
      .join('');
  }

  stageClass(stage: string): string {
    if (stage === 'Closed Won') return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';
    if (stage === 'Closed Lost') return 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300';
    if (stage === 'Negotiation' || stage === 'Proposal') return 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300';
    return 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300';
  }

  kindClass(kind: string): string {
    switch (kind) {
      case 'Payment': return 'text-emerald-600';
      case 'Invoice': return 'text-amber-600';
      case 'Order': return 'text-blue-600';
      case 'Opportunity': return 'text-indigo-600';
      case 'Activity': return 'text-sky-600';
      default: return 'text-[var(--text-muted)]';
    }
  }
}
