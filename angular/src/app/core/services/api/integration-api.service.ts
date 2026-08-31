import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
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

  private get basePath(): string {
    return `${this.apiRoot}/integration-config`;
  }

  getConfigs(): Promise<IntegrationConfig[]> {
    return firstValueFrom(
      this.http.get<{ items: IntegrationConfig[] }>(`${this.basePath}`)
    ).then(res => res.items ?? []);
  }

  async saveConfig(data: Partial<IntegrationConfig>): Promise<void> {
    if (data.id) {
      await firstValueFrom(this.http.put(`${this.basePath}/${data.id}`, data));
    } else {
      await firstValueFrom(this.http.post(`${this.basePath}`, data));
    }
  }

  async testConnection(id: string): Promise<boolean> {
    return firstValueFrom(this.http.post<boolean>(`${this.basePath}/${id}/test-connection`, {}));
  }

  async getActiveProviders(): Promise<IntegrationConfig[]> {
    return firstValueFrom(
      this.http.get<{ items: IntegrationConfig[] }>(`${this.basePath}/active-providers`)
    ).then(res => res.items ?? []);
  }
}
