import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { environment } from '../../../../environments/environment';

export interface ExpenseRequest {
  id: string;
  expenseCode: string;
  employeeName: string;
  category: 'Travel' | 'Meals' | 'Office Supplies' | 'Maintenance' | 'Other';
  amount: number;
  description: string;
  date: string;
  status: 'Pending Approval' | 'Approved' | 'Reimbursed';
}

export interface Project {
  id: string;
  code: string;
  name: string;
  clientName: string;
  budget: number;
  spentAmount: number;
  progressPercentage: number;
  status: 'In Progress' | 'Completed' | 'On Hold';
  deadline: string;
}

export interface ManufacturingOrder {
  id: string;
  moNumber: string;
  finishedProductName: string;
  quantityToProduce: number;
  workCenter: string;
  scheduledStartDate: string;
  status: 'Draft' | 'In Production' | 'QC Passed' | 'Completed';
}

export interface FixedAsset {
  id: string;
  assetCode: string;
  name: string;
  category: string;
  purchaseDate: string;
  purchaseCost: number;
  currentValue: number;
  depreciationRateAnnual: number;
  location: string;
  assignedEmployee: string;
}

export interface MaintenanceRequest {
  id: string;
  workOrderCode: string;
  assetName: string;
  maintenanceType: 'Preventive' | 'Corrective';
  technicianName: string;
  cost: number;
  status: 'Open' | 'In Progress' | 'Completed';
  scheduledDate: string;
}

@Injectable({ providedIn: 'root' })
export class EnterpriseApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  // Expenses
  getExpenses(): Promise<ExpenseRequest[]> {
    return this.getList<ExpenseRequest>('expense');
  }
  createExpense(e: Partial<ExpenseRequest>): Promise<void> { return this.post('expense', e); }
  approveExpense(id: string): Promise<void> { return this.post(`expense/${id}/approve`, {}); }

  // Projects
  getProjects(): Promise<Project[]> {
    return this.getList<Project>('project');
  }
  createProject(p: Partial<Project>): Promise<void> { return this.post('project', p); }

  // Manufacturing Orders
  getManufacturingOrders(): Promise<ManufacturingOrder[]> {
    return this.getList<ManufacturingOrder>('manufacturing');
  }
  createManufacturingOrder(m: Partial<ManufacturingOrder>): Promise<void> { return this.post('manufacturing', m); }

  // Assets
  getFixedAssets(): Promise<FixedAsset[]> {
    return this.getList<FixedAsset>('asset');
  }
  createFixedAsset(a: Partial<FixedAsset>): Promise<void> { return this.post('asset', a); }

  // Maintenance
  getMaintenanceRequests(): Promise<MaintenanceRequest[]> {
    return this.getList<MaintenanceRequest>('maintenance');
  }
  createMaintenanceRequest(m: Partial<MaintenanceRequest>): Promise<void> { return this.post('maintenance', m); }
}
