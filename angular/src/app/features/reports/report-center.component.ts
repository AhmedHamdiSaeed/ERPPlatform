import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number)
import { FormsModule } from '@angular/forms';
import { ReportDefinition } from '../../core/models/erp-models';
import { MOCK_REPORTS } from '../../core/mock/mock-data';

@Component({
  selector: 'app-report-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">Report Center & Business Intelligence</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">Generate, schedule, and export enterprise data reports across all ERP modules.</p>
        </div>

        <div class="flex items-center gap-2">
          <select [(ngModel)]="categoryFilter" class="px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs">
            <option value="ALL">All Modules</option>
            <option value="HR">Human Resources</option>
            <option value="Inventory">Inventory</option>
            <option value="Workflow">Workflow Engine</option>
            <option value="Financial">Financial</option>
          </select>
        </div>
      </div>

      <!-- Report Cards Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        @for (rep of filteredReports(); track rep.id || $index) {
          <div class="card-panel hover:border-blue-500 transition-all group flex flex-col justify-between">
            
            <div class="space-y-3">
              <div class="flex items-start justify-between">
                <span class="px-2.5 py-0.5 text-[10px] font-bold rounded-full"
                  [class.bg-blue-100]="rep.category === 'HR'" [class.text-blue-700]="rep.category === 'HR'"
                  [class.bg-emerald-100]="rep.category === 'Inventory'" [class.text-emerald-700]="rep.category === 'Inventory'"
                  [class.bg-indigo-100]="rep.category === 'Workflow'" [class.text-indigo-700]="rep.category === 'Workflow'"
                  [class.bg-amber-100]="rep.category === 'Financial'" [class.text-amber-700]="rep.category === 'Financial'">
                  {{ rep.category }}
                </span>
                <span class="text-[10px] text-slate-400 font-mono">{{ rep.recordCount | number }} records</span>
              </div>

              <h3 class="font-extrabold text-sm text-[var(--text-main)] group-hover:text-blue-600 transition-colors leading-tight">{{ rep.title }}</h3>
              <p class="text-xs text-[var(--text-muted)] leading-relaxed">{{ rep.description }}</p>
            </div>

            <div class="pt-4 mt-4 border-t border-[var(--border-color)] flex items-center justify-between">
              <span class="text-[11px] text-slate-400"><i class="pi pi-clock text-[10px]"></i> Last run: {{ rep.lastGenerated }}</span>
              <div class="flex items-center gap-2">
                <button (click)="generateReport(rep)" class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-[11px] rounded-xl transition-colors">
                  <i class="pi pi-chart-bar text-[10px]"></i> Generate
                </button>
                <button (click)="exportReport(rep)" class="px-3 py-1.5 bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 font-bold text-[11px] rounded-xl hover:bg-slate-200 transition-colors">
                  <i class="pi pi-download text-[10px]"></i> Export CSV
                </button>
              </div>
            </div>

          </div>
        }
      </div>

      <!-- Generated Report Preview Modal -->
      @if (previewReport()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-fade-in">
          <div class="w-full max-w-3xl bg-[var(--bg-card)] border border-[var(--border-color)] rounded-2xl shadow-2xl p-6 space-y-5 overflow-y-auto max-h-[85vh]">
            
            <div class="flex items-center justify-between border-b border-[var(--border-color)] pb-3">
              <div>
                <h3 class="font-extrabold text-base text-[var(--text-main)]">{{ previewReport()?.title }}</h3>
                <p class="text-xs text-[var(--text-muted)]">Generated on {{ today }} • {{ previewReport()?.recordCount | number }} records processed</p>
              </div>
              <button (click)="previewReport.set(null)" class="text-slate-400 hover:text-slate-600"><i class="pi pi-times text-lg"></i></button>
            </div>

            <!-- Sample Chart Area -->
            <div class="bg-slate-50 dark:bg-slate-800/50 rounded-xl p-8 text-center space-y-3 border border-dashed border-slate-300 dark:border-slate-700">
              <i class="pi pi-chart-bar text-5xl text-blue-500"></i>
              <p class="text-xs text-slate-500 font-medium">Data visualization for <strong>{{ previewReport()?.title }}</strong> would render here using Chart.js or PrimeNG charts.</p>
              <p class="text-[10px] text-slate-400">In production, this connects to the ABP backend reporting engine via REST APIs.</p>
            </div>

            <!-- Sample Data Table -->
            <div class="space-y-2">
              <h4 class="text-xs font-bold text-[var(--text-main)]">Sample Data Preview (Top 5 Records)</h4>
              <div class="border border-[var(--border-color)] rounded-xl overflow-hidden">
                <table class="w-full text-left text-xs">
                  <thead class="bg-slate-100 dark:bg-slate-800 font-bold text-slate-500">
                    <tr>
                      <th class="p-2.5">#</th>
                      <th class="p-2.5">Metric</th>
                      <th class="p-2.5">Value</th>
                      <th class="p-2.5">Trend</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-[var(--border-color)]">
                    @for (row of sampleRows; track row.metric) {
                      <tr>
                        <td class="p-2.5 font-mono text-slate-400">{{ $index + 1 }}</td>
                        <td class="p-2.5 font-semibold">{{ row.metric }}</td>
                        <td class="p-2.5 font-bold text-blue-600">{{ row.value }}</td>
                        <td class="p-2.5">
                          <span [class.text-emerald-600]="row.trend === 'up'" [class.text-rose-600]="row.trend === 'down'" class="font-bold">
                            <i [class]="'pi ' + (row.trend === 'up' ? 'pi-arrow-up' : 'pi-arrow-down')"></i> {{ row.change }}
                          </span>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>

            <div class="pt-3 border-t border-[var(--border-color)] flex justify-end gap-2">
              <button (click)="previewReport.set(null)" class="btn-outline text-xs">Close</button>
              <button class="btn-primary text-xs"><i class="pi pi-download"></i> Export Full Report</button>
            </div>

          </div>
        </div>
      }

    </div>
  `
})
export class ReportCenterComponent {
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
    alert(`Exporting "${rep.title}" as CSV... (${rep.recordCount} records)`);
  }
}
