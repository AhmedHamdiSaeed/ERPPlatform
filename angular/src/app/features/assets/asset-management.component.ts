import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnterpriseApiService, FixedAsset, MaintenanceRequest } from '../../core/services/api/enterprise-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-asset-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './asset-management.component.html'
})
export class AssetManagementComponent {
  private enterpriseApi = inject(EnterpriseApiService);
  private toast = inject(ToastService);

  activeTab = signal<'assets' | 'maintenance'>('assets');

  assets = signal<FixedAsset[]>([]);
  maintenanceRequests = signal<MaintenanceRequest[]>([]);

  showAssetModal = signal(false);
  showMaintModal = signal(false);

  newAsset: Partial<FixedAsset> = { category: 'Machinery', depreciationRateAnnual: 15, location: 'Cairo Plant' };
  newMaint: Partial<MaintenanceRequest> = { maintenanceType: 'Preventive', status: 'In Progress' };

  constructor() {
    this.loadData();
  }

  async loadData() {
    const [a, m] = await Promise.all([
      this.enterpriseApi.getFixedAssets(),
      this.enterpriseApi.getMaintenanceRequests()
    ]);
    this.assets.set(a);
    this.maintenanceRequests.set(m);
  }

  async saveAsset() {
    await this.enterpriseApi.createFixedAsset(this.newAsset);
    this.toast.success('Fixed Asset registered into asset master ledger.');
    this.showAssetModal.set(false);
    await this.loadData();
  }

  async saveMaintenance() {
    await this.enterpriseApi.createMaintenanceRequest(this.newMaint);
    this.toast.success('Equipment maintenance work order dispatched.');
    this.showMaintModal.set(false);
    await this.loadData();
  }
}
