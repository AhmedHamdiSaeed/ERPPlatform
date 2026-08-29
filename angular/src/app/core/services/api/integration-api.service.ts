import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface IntegrationConfig {
  id: string;
  providerType: string;
  providerName: string;
  apiKey: string;
  secretKey: string;
  webhookSecret: string;
  endpointUrl: string;
  isActive: boolean;
  configJson: string;
}

@Injectable({
  providedIn: 'root'
})
export class IntegrationApiService extends ErpApiService {

  getConfigs(): Promise<IntegrationConfig[]> {
    return this.getList<IntegrationConfig>('');
  }

  async saveConfig(data: Partial<IntegrationConfig>): Promise<void> {
    if (data.id) {
      await this.put(`${data.id}`, data);
    } else {
      await this.post('', data);
    }
  }

  async testConnection(id: string): Promise<boolean> {
    return this.post<boolean>(`${id}/test-connection`, {});
  }
}
