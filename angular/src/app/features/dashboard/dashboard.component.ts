import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../core/services/state.service';
import { AiService } from '../../core/services/ai.service';
import { DashboardApiService, DashboardStats } from '../../core/services/api/dashboard-api.service';
import { ChartModule } from 'primeng/chart';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ChartModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  state = inject(StateService);
  aiService = inject(AiService);
  private dashboardApi = inject(DashboardApiService);

  showAiModal = signal(false);
  aiLoading = signal(false);
  aiSummary = signal('');

  stats = signal<DashboardStats | null>(null);

  constructor() {
    this.loadStats();
  }

  async loadStats() {
    const data = await this.dashboardApi.getDashboardStats();
    this.stats.set(data);
  }

  // Department Bar Chart Data
  deptChartData = {
    labels: ['IT', 'HR', 'Finance', 'Sales', 'Logistics', 'QA', 'Support'],
    datasets: [
      {
        label: 'Employee Count',
        data: [42, 18, 24, 56, 65, 15, 25],
        backgroundColor: '#2563eb',
        borderRadius: 6
      }
    ]
  };

  // Workflow Doughnut Chart Data
  workflowChartData = {
    labels: ['Completed', 'Running / Pending', 'Failed'],
    datasets: [
      {
        data: [82, 15, 3],
        backgroundColor: ['#10b981', '#f59e0b', '#ef4444']
      }
    ]
  };

  chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } }
  };

  doughnutOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } }
  };

  activities = [
    { user: 'Ahmed Hamdi', action: 'created purchase order', detail: 'PO-2026-8801 for TechSupply Co. ($18,880)', time: '10 mins ago', icon: 'pi-file', bg: 'bg-blue-100 text-blue-600' },
    { user: 'Sara Mahmoud', action: 'approved leave request for', detail: 'Mona Zaki (2 days Sick Leave)', time: '25 mins ago', icon: 'pi-check', bg: 'bg-emerald-100 text-emerald-600' },
    { user: 'System Sentinel', action: 'detected low stock alert on', detail: 'Logitech MX Master 3S Mouse (8 left)', time: '1 hour ago', icon: 'pi-exclamation-triangle', bg: 'bg-amber-100 text-amber-600' },
    { user: 'Omar Farouk', action: 'initiated stock transfer', detail: 'TRF-2026-002 from Central Hub to Main WH', time: '3 hours ago', icon: 'pi-sync', bg: 'bg-indigo-100 text-indigo-600' }
  ];

  approvalCenterLink = '/workflow/approvals';

  openAiAnalysis() {
    this.showAiModal.set(true);
    this.aiLoading.set(true);
    this.aiService.getDashboardAiAnalysis().subscribe(summary => {
      this.aiSummary.set(summary);
      this.aiLoading.set(false);
    });
  }
}
