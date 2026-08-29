import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export interface DashboardStats {
  totalEmployees: number;
  activeEmployees: number;
  onLeaveCount: number;
  totalDepartments: number;
  totalProducts: number;
  lowStockCount: number;
  outOfStockCount: number;
  inventoryValue: number;
  pendingLeavesCount: number;
  totalWarehouses: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  getDashboardStats(): Promise<DashboardStats> {
    return this.get<DashboardStats>('dashboard-stats/stats');
  }
}
