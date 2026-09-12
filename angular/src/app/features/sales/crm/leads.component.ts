import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CrmApiService, Lead } from '../../../core/services/api/crm-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-leads',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './leads.component.html'
})
export class LeadsComponent {
  private crmApi = inject(CrmApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
  private router = inject(Router);

  leads = signal<Lead[]>([]);
  statusFilter = signal<string>('ALL');
  showModal = signal(false);

  newLead: Partial<Lead> = {
    name: '',
    companyName: '',
    email: '',
    phone: '',
    source: 'Website',
    status: 'New',
    salespersonName: 'Ahmed Hamdi',
    nextFollowUp: new Date().toISOString().split('T')[0]
  };

  constructor() {
    this.loadLeads();
  }

  async loadLeads() {
    this.leads.set(await this.crmApi.getLeads());
  }

  filteredLeads = computed(() => {
    const list = this.leads();
    const filter = this.statusFilter();
    return filter === 'ALL' ? list : list.filter(l => l.status === filter);
  });

  scoreClass(score: number): string {
    if (score >= 70) return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300';
    if (score >= 40) return 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300';
    return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
  }

  openAddModal() {
    this.newLead = {
      name: '',
      companyName: '',
      email: '',
      phone: '',
      source: 'Website',
      status: 'New',
      salespersonName: 'Ahmed Hamdi',
      nextFollowUp: new Date().toISOString().split('T')[0]
    };
    this.showModal.set(true);
  }

  async saveLead() {
    await this.crmApi.createLead(this.newLead);
    this.toast.success('CRM Lead created successfully.');
    this.showModal.set(false);
    await this.loadLeads();
  }

  async qualify(id: string) {
    await this.crmApi.qualifyLead(id);
    this.toast.success('Lead marked as qualified.');
    await this.loadLeads();
  }

  async convertToOpportunity(id: string) {
    await this.crmApi.convertToOpportunity(id);
    this.toast.success('Lead converted to Sales Opportunity!');
    await this.loadLeads();
  }

  async markUnqualified(id: string, name: string) {
    const reason = await this.dialog.prompt({
      title: 'Mark Lead as Lost',
      message: `Why is "${name}" not a fit?`,
      placeholder: 'Reason for Loss',
      defaultValue: 'Not a good fit',
      confirmText: 'Submit',
      type: 'warning',
      icon: 'pi-times-circle'
    });
    if (reason === null) return; // cancelled
    await this.crmApi.markUnqualified(id, reason || 'Not a good fit');
    this.toast.success('Lead marked as unqualified.');
    await this.loadLeads();
  }

  openAccount(lead: Lead) {
    if (lead.convertedCustomerId) {
      this.router.navigate(['/sales/crm/customer', lead.convertedCustomerId]);
    }
  }

  async deleteLead(id: string) {
    const confirmed = await this.dialog.confirm({
      title: 'Delete Lead',
      message: 'Are you sure you want to delete this sales lead?',
      confirmText: 'Delete',
      type: 'danger'
    });
    if (confirmed) {
      await this.crmApi.deleteLead(id);
      this.toast.success('Lead removed.');
      await this.loadLeads();
    }
  }
}
