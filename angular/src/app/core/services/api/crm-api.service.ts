import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

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
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  getLeads(): Promise<Lead[]> {
    return this.getList<Lead>('lead');
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
