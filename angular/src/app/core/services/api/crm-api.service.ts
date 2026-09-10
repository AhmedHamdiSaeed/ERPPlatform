import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Converted' | 'Lost';

export interface Lead {
  id: string;
  name: string;
  companyName: string;
  email: string;
  phone: string;
  source: string;
  status: LeadStatus;
  salespersonName: string;
  nextFollowUp: string;
  notes?: string;

  // Qualification & conversion
  ownerUserId?: string | null;
  score: number;
  lostReason?: string;
  qualifiedAt?: string | null;
  convertedAt?: string | null;
  convertedCustomerId?: string | null;
  convertedDealId?: string | null;
  tags?: string;
  creationTime?: string;
}

export interface CrmContact {
  id: string;
  customerId?: string | null;
  leadId?: string | null;
  firstName: string;
  lastName: string;
  fullName: string;
  jobTitle: string;
  email: string;
  phone: string;
  mobile: string;
  isPrimary: boolean;
  notes?: string;
  customerName?: string;
  creationTime?: string;
}

export type CrmActivityType = 'Call' | 'Meeting' | 'Task' | 'Email' | 'FollowUp';

export interface CrmActivity {
  id: string;
  type: CrmActivityType;
  subject: string;
  description: string;
  dueDate: string;
  status: 'Open' | 'Completed' | 'Cancelled';
  completedAt?: string | null;
  priority: 'Low' | 'Normal' | 'High';
  leadId?: string | null;
  customerId?: string | null;
  contactId?: string | null;
  dealId?: string | null;
  assignedTo: string;
  assignedToUserId?: string | null;
  outcome?: string;
  creationTime?: string;
  isOverdue: boolean;
  customerName?: string;
  leadName?: string;
  dealTitle?: string;
}

export interface CrmNote {
  id: string;
  leadId?: string | null;
  customerId?: string | null;
  contactId?: string | null;
  dealId?: string | null;
  content: string;
  isPinned: boolean;
  attachmentName?: string;
  attachmentUrl?: string;
  creationTime?: string;
}

export interface PipelineStage {
  stage: string;
  dealCount: number;
  value: number;
}

export interface LeadSourceStat {
  source: string;
  count: number;
  convertedCount: number;
}

export interface ActivityByOwner {
  owner: string;
  total: number;
  open: number;
  overdue: number;
}

export interface CrmDashboard {
  totalLeads: number;
  newLeads: number;
  contactedLeads: number;
  qualifiedLeads: number;
  convertedLeads: number;
  lostLeads: number;

  openOpportunities: number;
  wonDeals: number;
  lostDeals: number;
  pipelineValue: number;
  weightedForecast: number;
  wonValue: number;

  leadConversionRatePercentage: number;
  winRatePercentage: number;
  averageDealValue: number;
  averageSalesCycleDays: number;

  openActivities: number;
  overdueActivities: number;
  callsThisMonth: number;
  meetingsThisMonth: number;
  tasksThisMonth: number;

  pipelineByStage: PipelineStage[];
  leadsBySource: LeadSourceStat[];
  activitiesByOwner: ActivityByOwner[];
  upcomingActivities: CrmActivity[];
}

export interface TimelineItem {
  at: string;
  kind: 'Activity' | 'Note' | 'Opportunity' | 'Quotation' | 'Order' | 'Invoice' | 'Payment';
  title: string;
  detail: string;
  status: string;
  amount?: number | null;
  referenceId?: string | null;
}

export interface Customer360 {
  customer: {
    id: string;
    customerCode: string;
    name: string;
    email: string;
    phone: string;
    address: string;
    contactPerson: string;
    taxNumber: string;
    industry: string;
    website: string;
    ownerName: string;
    isVip: boolean;
    isActive: boolean;
    tags: string;
    creditLimit: number;
    outstandingBalance: number;
    paymentTerms: string;
    currency: string;
  };
  contacts: CrmContact[];
  opportunities: Array<{
    id: string;
    title: string;
    customerName: string;
    value: number;
    stage: string;
    probability: number;
    expectedCloseDate: string;
    ownerName: string;
    competitor?: string;
    lostReason?: string;
    closedAt?: string | null;
    creationTime?: string;
  }>;
  activities: CrmActivity[];
  notes: CrmNote[];
  timeline: TimelineItem[];
  quotations: Array<{ id: string; number: string; date: string; totalAmount: number; status: string }>;
  orders: Array<{ id: string; number: string; date: string; totalAmount: number; status: string }>;
  invoices: Array<{ id: string; number: string; date: string; dueDate: string; totalAmount: number; status: string }>;
  payments: Array<{ id: string; number: string; date: string; amount: number; method: string; status: string }>;

  openOpportunityCount: number;
  pipelineValue: number;
  orderCount: number;
  orderValue: number;
  invoiceCount: number;
  invoicedValue: number;
  outstandingValue: number;
  paidValue: number;
  openActivityCount: number;
  overdueActivityCount: number;
}

export interface ActivityFilter {
  customerId?: string;
  leadId?: string;
  dealId?: string;
  type?: string;
  status?: string;
  onlyOverdue?: boolean;
}

@Injectable({ providedIn: 'root' })
export class CrmApiService extends ErpApiService {
  /**
   * Leads are served from the /api/hr root (historical routing). Everything else
   * added for CRM Phase 1 lives under the default /api/app root.
   */
  private get leadUrl(): string {
    return `${environment.apis.default.url}/api/hr/lead`;
  }

  private leadPaged(route: string): Promise<Lead[]> {
    return firstValueFrom(
      this.http.get<{ items: Lead[] }>(
        `${this.leadUrl}${route}${route.includes('?') ? '&' : '?'}MaxResultCount=1000`
      )
    ).then(res => res.items ?? []);
  }

  // ── Leads ────────────────────────────────────────────────────────

  getLeads(status?: string, filter?: string): Promise<Lead[]> {
    const q = new URLSearchParams();
    if (status && status !== 'ALL') q.set('Status', status);
    if (filter) q.set('Filter', filter);
    const query = q.toString();
    return this.leadPaged(query ? `?${query}` : '');
  }

  getOverdueFollowUps(): Promise<Lead[]> {
    return this.leadPaged('?OnlyOverdueFollowUp=true');
  }

  createLead(lead: Partial<Lead>): Promise<Lead> {
    return firstValueFrom(this.http.post<Lead>(this.leadUrl, lead));
  }

  updateLead(id: string, lead: Partial<Lead>): Promise<Lead> {
    return firstValueFrom(this.http.put<Lead>(`${this.leadUrl}/${id}`, lead));
  }

  deleteLead(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.leadUrl}/${id}`));
  }

  /** New → Contacted */
  markContacted(id: string): Promise<Lead> {
    return firstValueFrom(this.http.post<Lead>(`${this.leadUrl}/${id}/mark-contacted`, {}));
  }

  /** Contacted → Qualified */
  qualifyLead(id: string): Promise<Lead> {
    return firstValueFrom(this.http.post<Lead>(`${this.leadUrl}/${id}/qualify`, {}));
  }

  markUnqualified(id: string, reason: string): Promise<Lead> {
    return firstValueFrom(
      this.http.post<Lead>(`${this.leadUrl}/${id}/mark-unqualified`, { reason })
    );
  }

  assignLead(id: string, salespersonName: string): Promise<Lead> {
    return firstValueFrom(
      this.http.post<Lead>(`${this.leadUrl}/${id}/assign`, { salespersonName })
    );
  }

  /** Creates (or reuses) the account, the primary contact and the opportunity. */
  convertToOpportunity(id: string): Promise<Lead> {
    return firstValueFrom(
      this.http.post<Lead>(`${this.leadUrl}/${id}/convert-to-opportunity`, {})
    );
  }

  // ── Contacts ─────────────────────────────────────────────────────

  getContacts(customerId?: string, filter?: string): Promise<CrmContact[]> {
    const q = new URLSearchParams();
    if (customerId) q.set('CustomerId', customerId);
    if (filter) q.set('Filter', filter);
    const query = q.toString();
    return this.getList<CrmContact>(query ? `crm-contact?${query}` : 'crm-contact');
  }

  createContact(c: Partial<CrmContact>): Promise<CrmContact> {
    return this.post<CrmContact>('crm-contact', c);
  }

  updateContact(id: string, c: Partial<CrmContact>): Promise<CrmContact> {
    return this.put<CrmContact>(`crm-contact/${id}`, c);
  }

  deleteContact(id: string): Promise<void> {
    return this.delete(`crm-contact/${id}`);
  }

  makePrimaryContact(id: string): Promise<CrmContact> {
    return this.post<CrmContact>(`crm-contact/${id}/make-primary`, {});
  }

  // ── Activities ───────────────────────────────────────────────────

  getActivities(filter: ActivityFilter = {}): Promise<CrmActivity[]> {
    const q = new URLSearchParams();
    if (filter.customerId) q.set('CustomerId', filter.customerId);
    if (filter.leadId) q.set('LeadId', filter.leadId);
    if (filter.dealId) q.set('DealId', filter.dealId);
    if (filter.type) q.set('Type', filter.type);
    if (filter.status) q.set('Status', filter.status);
    if (filter.onlyOverdue) q.set('OnlyOverdue', 'true');
    const query = q.toString();
    return this.getList<CrmActivity>(query ? `crm-activity?${query}` : 'crm-activity');
  }

  createActivity(a: Partial<CrmActivity>): Promise<CrmActivity> {
    return this.post<CrmActivity>('crm-activity', a);
  }

  completeActivity(id: string, outcome = ''): Promise<CrmActivity> {
    return this.post<CrmActivity>(`crm-activity/${id}/complete`, { outcome });
  }

  reopenActivity(id: string): Promise<CrmActivity> {
    return this.post<CrmActivity>(`crm-activity/${id}/reopen`, {});
  }

  deleteActivity(id: string): Promise<void> {
    return this.delete(`crm-activity/${id}`);
  }

  // ── Notes ────────────────────────────────────────────────────────

  getNotes(customerId?: string, dealId?: string): Promise<CrmNote[]> {
    const q = new URLSearchParams();
    if (customerId) q.set('CustomerId', customerId);
    if (dealId) q.set('DealId', dealId);
    const query = q.toString();
    return this.getList<CrmNote>(query ? `crm-note?${query}` : 'crm-note');
  }

  createNote(n: Partial<CrmNote>): Promise<CrmNote> {
    return this.post<CrmNote>('crm-note', n);
  }

  deleteNote(id: string): Promise<void> {
    return this.delete(`crm-note/${id}`);
  }

  // ── Dashboard & Customer 360 ─────────────────────────────────────

  getDashboard(): Promise<CrmDashboard> {
    return this.get<CrmDashboard>('crm-dashboard/kpis');
  }

  /** Note: ABP renders the route as `customer360` (digits are not split). */
  getCustomer360(id: string): Promise<Customer360> {
    return this.get<Customer360>(`customer360/${id}`);
  }
}
