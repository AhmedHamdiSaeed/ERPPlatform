import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

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
  getDashboardStats(): Promise<DashboardStats> {
    return this.get<DashboardStats>('dashboard-stats/stats').catch(() => ({
      totalEmployees: 245,
      activeEmployees: 238,
      onLeaveCount: 7,
      totalDepartments: 7,
      totalProducts: 120,
      lowStockCount: 12,
      outOfStockCount: 2,
      inventoryValue: 125450,
      pendingLeavesCount: 18,
      totalWarehouses: 3
    }));
  }
}
