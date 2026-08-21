import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number)
import { FormsModule } from '@angular/forms';
import { ReportDefinition } from '../../core/models/erp-models';
import { MOCK_REPORTS } from '../../core/mock/mock-data';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-report-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report-center.component.html'
})
export class ReportCenterComponent {
  private toast = inject(ToastService);

  reports = signal<ReportDefinition[]>(MOCK_REPORTS);
  categoryFilter = 'ALL';
  previewReport = signal<ReportDefinition | null>(null);
  today = new Date().toISOString().split('T')[0];

  sampleRows = [
    { metric: 'Total Headcount', value: '245', trend: 'up', change: '+3.3%' },
    { metric: 'Inventory Valuation', value: '$125,450', trend: 'up', change: '+4.5%' },
    { metric: 'Avg. Approval Time', value: '4.2h', trend: 'down', change: '-18%' },
    { metric: 'Open Purchase Orders', value: '12', trend: 'up', change: '+2' },
    { metric: 'Workflow Throughput', value: '142 runs/wk', trend: 'up', change: '+14%' }
  ];

  filteredReports() {
    if (this.categoryFilter === 'ALL') return this.reports();
    return this.reports().filter(r => r.category === this.categoryFilter);
  }

  generateReport(rep: ReportDefinition) {
    this.previewReport.set(rep);
  }

  exportReport(rep: ReportDefinition) {
    this.toast.info(`Exporting "${rep.title}" CSV file (${rep.recordCount} records)...`, 'Report Export');
  }
}
