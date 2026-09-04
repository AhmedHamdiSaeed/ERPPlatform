import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { IntegrationApiService, IntegrationConfig } from '../../../core/services/api/integration-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-integrations',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './integrations.component.html'
})
export class IntegrationsComponent {
  private api = inject(IntegrationApiService);
  private toast = inject(ToastService);

  configs = signal<IntegrationConfig[]>([]);
  loading = signal(false);

  showModal = signal(false);
  currentConfig: Partial<IntegrationConfig> = {};

  constructor() {
    this.loadConfigs();
  }

  async loadConfigs() {
    this.loading.set(true);
    try {
      this.configs.set(await this.api.getConfigs());
    } finally {
      this.loading.set(false);
    }
  }

  openEditModal(config: IntegrationConfig) {
    this.currentConfig = { ...config };
    this.showModal.set(true);
  }

  openNewModal() {
    this.currentConfig = {
      providerType: 'PaymentGateway',
      providerName: 'Stripe',
      apiKey: '',
      secretKey: '',
      webhookSecret: '',
      endpointUrl: '',
      isActive: true
    };
    this.showModal.set(true);
  }

  async saveConfig() {
    try {
      await this.api.saveConfig(this.currentConfig);
      this.toast.success('Integration settings saved successfully.');
      this.showModal.set(false);
      await this.loadConfigs();
    } catch {
      this.toast.error('Failed to save integration configuration.');
    }
  }

  async testConnection(config: IntegrationConfig) {
    this.toast.info(`Testing connection to ${config.providerName}...`);
    const success = await this.api.testConnection(config.id);
    if (success) {
      this.toast.success(`Connection to ${config.providerName} verified!`);
    } else {
      this.toast.error(`Could not connect to ${config.providerName}. Check API keys.`);
    }
  }
}
