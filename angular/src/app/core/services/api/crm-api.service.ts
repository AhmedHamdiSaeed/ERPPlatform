import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface Lead {
  id: string;
  name: string;
  companyName: string;
  email: string;
  phone: string;
  source: string;
  status: 'New' | 'Qualified' | 'Opportunity' | 'Converted' | 'Lost';
  salespersonName: string;
  nextFollowUp: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class CrmApiService extends ErpApiService {
  getLeads(): Promise<Lead[]> {
    return this.getList<Lead>('lead').catch(() => [
      { id: 'ld-1', name: 'Tarek Mansour', companyName: 'Apex Logistics LLC', email: 'tarek@apex.com', phone: '+20 100 999 8877', source: 'Website', status: 'Qualified', salespersonName: 'Nour El-Din', nextFollowUp: '2026-08-30', notes: 'Interested in ERP inventory & sales modules.' },
      { id: 'ld-2', name: 'Reem El-Sayed', companyName: 'Delta Retail Group', email: 'reem@deltaretail.io', phone: '+20 102 777 6655', source: 'Referral', status: 'Opportunity', salespersonName: 'Ahmed Hamdi', nextFollowUp: '2026-09-02', notes: 'Scheduled software demo call.' },
      { id: 'ld-3', name: 'Hassan Mahmoud', companyName: 'Nile Industrial Co.', email: 'hassan@nileind.com', phone: '+20 111 444 3322', source: 'Social Media', status: 'New', salespersonName: 'Nour El-Din', nextFollowUp: '2026-08-31' }
    ]);
  }

  createLead(lead: Partial<Lead>): Promise<void> {
    return this.post('lead', lead);
  }

  convertToOpportunity(id: string): Promise<void> {
    return this.post(`lead/${id}/convert-to-opportunity`, {});
  }

  deleteLead(id: string): Promise<void> {
    return this.delete(`lead/${id}`);
  }
}
