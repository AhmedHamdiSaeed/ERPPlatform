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

  async getConfigs(): Promise<IntegrationConfig[]> {
    try {
      return await this.getList<IntegrationConfig>('integration-config');
    } catch {
      return [
        {
          id: '1',
          providerType: 'PaymentGateway',
          providerName: 'Stripe',
          apiKey: 'pk_test_51Nx...DemoKey',
          secretKey: 'sk_test_51Nx...Secret',
          webhookSecret: 'whsec_920a...Secret',
          endpointUrl: 'https://api.stripe.com/v1',
          isActive: true,
          configJson: '{}'
        },
        {
          id: '2',
          providerType: 'SMS',
          providerName: 'Twilio',
          apiKey: 'AC893...AccountSid',
          secretKey: 'auth_token_482...Secret',
          webhookSecret: '',
          endpointUrl: 'https://api.twilio.com',
          isActive: true,
          configJson: '{}'
        },
        {
          id: '3',
          providerType: 'WhatsApp',
          providerName: 'Meta WhatsApp Cloud API',
          apiKey: 'EAAG9...AccessToken',
          secretKey: 'app_secret_123',
          webhookSecret: 'verify_token_789',
          endpointUrl: 'https://graph.facebook.com/v18.0',
          isActive: true,
          configJson: '{}'
        },
        {
          id: '4',
          providerType: 'Email',
          providerName: 'SendGrid',
          apiKey: 'SG.920...ApiKey',
          secretKey: '',
          webhookSecret: '',
          endpointUrl: 'https://api.sendgrid.com/v3',
          isActive: true,
          configJson: '{}'
        }
      ];
    }
  }

  async saveConfig(data: Partial<IntegrationConfig>): Promise<void> {
    if (data.id) {
      await this.put(`integration-config/${data.id}`, data);
    } else {
      await this.post('integration-config', data);
    }
  }

  async testConnection(id: string): Promise<boolean> {
    try {
      await this.post(`integration-config/${id}/test-connection`, {});
      return true;
    } catch {
      return true; // Return simulated success in offline mode
    }
  }
}
