import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CrmApiService, Lead } from '../../../core/services/api/crm-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-leads',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './leads.component.html'
})
export class LeadsComponent {
  private crmApi = inject(CrmApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

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

  async convertToOpportunity(id: string) {
    await this.crmApi.convertToOpportunity(id);
    this.toast.success('Lead converted to Sales Opportunity!');
    await this.loadLeads();
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
