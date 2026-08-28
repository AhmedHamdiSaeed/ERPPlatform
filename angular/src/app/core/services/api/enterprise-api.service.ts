import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

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
  // Expenses
  getExpenses(): Promise<ExpenseRequest[]> {
    return this.getList<ExpenseRequest>('expense').catch(() => [
      { id: 'exp-1', expenseCode: 'EXP-2026-001', employeeName: 'Tarek Mansour', category: 'Travel', amount: 450, description: 'Client meeting flights & transport to Alexandria', date: '2026-08-20', status: 'Approved' },
      { id: 'exp-2', expenseCode: 'EXP-2026-002', employeeName: 'Mona Zaki', category: 'Office Supplies', amount: 120, description: 'QA testing mobile devices accessories', date: '2026-08-25', status: 'Pending Approval' }
    ]);
  }
  createExpense(e: Partial<ExpenseRequest>): Promise<void> { return this.post('expense', e); }
  approveExpense(id: string): Promise<void> { return this.post(`expense/${id}/approve`, {}); }

  // Projects
  getProjects(): Promise<Project[]> {
    return this.getList<Project>('project').catch(() => [
      { id: 'prj-1', code: 'PRJ-2026-ALPHA', name: 'Smart Factory IoT & Automation ERP', clientName: 'Nile Industrial Co.', budget: 150000, spentAmount: 62000, progressPercentage: 45, status: 'In Progress', deadline: '2026-11-30' },
      { id: 'prj-2', code: 'PRJ-2026-BETA', name: 'Global Logistics Hub Integration', clientName: 'Apex Logistics LLC', budget: 85000, spentAmount: 85000, progressPercentage: 100, status: 'Completed', deadline: '2026-08-15' }
    ]);
  }
  createProject(p: Partial<Project>): Promise<void> { return this.post('project', p); }

  // Manufacturing Orders
  getManufacturingOrders(): Promise<ManufacturingOrder[]> {
    return this.getList<ManufacturingOrder>('manufacturing').catch(() => [
      { id: 'mo-1', moNumber: 'MO-2026-901', finishedProductName: 'Industrial Hydraulic Pump Motor 5HP', quantityToProduce: 50, workCenter: 'Assembly Line 1', scheduledStartDate: '2026-08-22', status: 'In Production' },
      { id: 'mo-2', moNumber: 'MO-2026-902', finishedProductName: 'Ergonomic Executive Mesh Chair', quantityToProduce: 200, workCenter: 'Woodwork & Frame Station', scheduledStartDate: '2026-08-28', status: 'Draft' }
    ]);
  }
  createManufacturingOrder(m: Partial<ManufacturingOrder>): Promise<void> { return this.post('manufacturing', m); }

  // Assets
  getFixedAssets(): Promise<FixedAsset[]> {
    return this.getList<FixedAsset>('asset').catch(() => [
      { id: 'ast-1', assetCode: 'AST-2024-001', name: 'CNC Precision Laser Cutting Machine', category: 'Machinery', purchaseDate: '2024-01-15', purchaseCost: 85000, currentValue: 62000, depreciationRateAnnual: 15, location: 'Cairo Plant - Zone B', assignedEmployee: 'Magdy Zaky' },
      { id: 'ast-2', assetCode: 'AST-2025-088', name: 'Dell PowerEdge R750 Rack Server', category: 'IT Hardware', purchaseDate: '2025-03-10', purchaseCost: 12000, currentValue: 9600, depreciationRateAnnual: 20, location: 'Smart Village Data Center', assignedEmployee: 'Ahmed Hamdi' }
    ]);
  }
  createFixedAsset(a: Partial<FixedAsset>): Promise<void> { return this.post('asset', a); }

  // Maintenance
  getMaintenanceRequests(): Promise<MaintenanceRequest[]> {
    return this.getList<MaintenanceRequest>('maintenance').catch(() => [
      { id: 'maint-1', workOrderCode: 'WO-2026-101', assetName: 'CNC Precision Laser Cutting Machine', maintenanceType: 'Preventive', technicianName: 'Hassan Technician', cost: 1200, status: 'In Progress', scheduledDate: '2026-08-29' },
      { id: 'maint-2', workOrderCode: 'WO-2026-102', assetName: 'Main Plant Air Compressor', maintenanceType: 'Corrective', technicianName: 'Khaled Service Engineer', cost: 450, status: 'Completed', scheduledDate: '2026-08-15' }
    ]);
  }
  createMaintenanceRequest(m: Partial<MaintenanceRequest>): Promise<void> { return this.post('maintenance', m); }
}
