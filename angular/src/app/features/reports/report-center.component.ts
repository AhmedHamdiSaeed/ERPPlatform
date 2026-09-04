import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportDefinition } from '../../core/models/erp-models';
import { ReportsApiService, ReportRunResult } from '../../core/services/api/reports-api.service';
import { ToastService } from '../../core/services/toast.service';
import { TranslationService } from '../../core/services/translation.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-report-center',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './report-center.component.html'
})
export class ReportCenterComponent {
  private reportsApi = inject(ReportsApiService);
  private toast = inject(ToastService);
  private translation = inject(TranslationService);

  reports = signal<ReportDefinition[]>([]);
  categoryFilter = 'ALL';
  previewReport = signal<ReportDefinition | null>(null);
  previewData = signal<ReportRunResult | null>(null);
  loading = signal(false);
  generating = signal(false);
  exporting = signal(false);
  loadError = signal<string | null>(null);

  constructor() {
    this.loadReports();
  }

  async loadReports() {
    this.loading.set(true);
    this.loadError.set(null);
    try {
      this.reports.set(await this.reportsApi.getReports());
    } catch (e) {
      console.error('Failed to load report definitions', e);
      this.loadError.set('Could not load report definitions from the server.');
      this.toast.error('Could not load report definitions from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  filteredReports() {
    if (this.categoryFilter === 'ALL') return this.reports();
    return this.reports().filter(r => r.category === this.categoryFilter);
  }

  displayReportCategory(category: string): string {
    return category === 'HR' ? 'Human Resources' : category;
  }

  reportRecordCountLabel(count: number): string {
    return `${count.toLocaleString()} ${this.translation.instant('records')}`;
  }

  reportLastRunLabel(lastGenerated: string): string {
    return `${this.translation.instant('Last run:')} ${lastGenerated || '-'}`;
  }

  async generateReport(rep: ReportDefinition) {
    this.generating.set(true);
    try {
      const data = await this.reportsApi.runReport(rep.id);
      this.previewReport.set(rep);
      this.previewData.set(data);
      this.toast.success(`Generated "${rep.title}" (${data.recordCount} rows).`);
      await this.loadReports();
    } catch (e) {
      console.error('Failed to run report', e);
      this.toast.error(`Could not generate "${rep.title}".`);
    } finally {
      this.generating.set(false);
    }
  }

  async exportReport(rep: ReportDefinition) {
    this.exporting.set(true);
    try {
      const data = await this.reportsApi.runReport(rep.id);
      const csv = this.toCsv(data);
      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const fileName = `${rep.title.replace(/[^a-z0-9]+/gi, '_').replace(/^_|_$/g, '').toLowerCase() || 'report'}_${new Date().toISOString().slice(0, 10)}.csv`;
      link.href = URL.createObjectURL(blob);
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(link.href);
      this.toast.success(`Exported "${rep.title}" as CSV.`);
      await this.loadReports();
    } catch (e) {
      console.error('Failed to export report', e);
      this.toast.error(`Could not export "${rep.title}".`);
    } finally {
      this.exporting.set(false);
    }
  }

  closePreview() {
    this.previewReport.set(null);
    this.previewData.set(null);
  }

  private toCsv(result: ReportRunResult): string {
    const esc = (v: string) => `"${(v ?? '').replace(/"/g, '""')}"`;
    const head = result.columns.map(esc).join(',');
    const body = result.rows.map(r => r.cells.map(c => esc(c ?? '')).join(',')).join('\n');
    return `${head}\n${body}`;
  }
}
