import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ChartModule } from 'primeng/chart';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CrmApiService, CrmDashboard, CrmActivity } from '../../../core/services/api/crm-api.service';
import { ToastService } from '../../../core/services/toast.service';

/**
 * CRM dashboard. Every number here comes from a single call to
 * GET /api/app/crm-dashboard/kpis — the KPIs are computed server-side so the
 * screen stays a pure renderer (no per-widget requests, no client-side drift).
 */
@Component({
  selector: 'app-crm-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ChartModule, TranslatePipe],
  templateUrl: './crm-dashboard.component.html'
})
export class CrmDashboardComponent {
  private crmApi = inject(CrmApiService);
  private toast = inject(ToastService);

  data = signal<CrmDashboard | null>(null);
  loading = signal(true);
  error = signal('');

  private readonly palette = [
    '#378ADD', '#7F77DD', '#EF9F27', '#1D9E75', '#E24B4A', '#D4537E', '#888780'
  ];

  barOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { callback: (value: unknown) => `$${this.money(Number(value))}` }
      }
    }
  };

  doughnutOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } }
  };

  constructor() {
    this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.data.set(await this.crmApi.getDashboard());
    } catch {
      this.error.set('Could not load CRM dashboard data from the server.');
      this.toast.error('Could not load CRM dashboard data from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  money(value: number): string {
    return (value ?? 0).toLocaleString('en-US', { maximumFractionDigits: 0 });
  }

  money2(value: number): string {
    return (value ?? 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  /** Pipeline value per stage. Stages with nothing in them are hidden. */
  stageChart = computed(() => {
    const d = this.data();
    if (!d) return { labels: [], datasets: [] };
    const stages = (d.pipelineByStage ?? []).filter(s => s.dealCount > 0 || s.value > 0);
    return {
      labels: stages.map(s => s.stage),
      datasets: [{
        label: 'Pipeline value',
        data: stages.map(s => s.value),
        backgroundColor: stages.map((_, i) => this.palette[i % this.palette.length]),
        borderRadius: 4
      }]
    };
  });

  /** Lead volume split by source. */
  sourceChart = computed(() => {
    const d = this.data();
    if (!d) return { labels: [], datasets: [] };
    const sources = (d.leadsBySource ?? []).filter(s => s.count > 0);
    return {
      labels: sources.map(s => s.source),
      datasets: [{
        data: sources.map(s => s.count),
        backgroundColor: sources.map((_, i) => this.palette[i % this.palette.length])
      }]
    };
  });

  /** Largest source share, used for the caption under the doughnut. */
  topSource = computed(() => {
    const list = this.data()?.leadsBySource ?? [];
    return list.length > 0 ? list[0] : null;
  });

  iconFor(type: string): string {
    switch (type) {
      case 'Call': return 'pi-phone';
      case 'Meeting': return 'pi-users';
      case 'Task': return 'pi-check-square';
      case 'Email': return 'pi-envelope';
      case 'FollowUp': return 'pi-clock';
      default: return 'pi-circle';
    }
  }

  /** "in 3 days" / "2 days overdue" — relative to today, whole days. */
  dueLabel(activity: CrmActivity): string {
    const due = new Date(activity.dueDate).getTime();
    if (isNaN(due)) return '';
    const days = Math.round((due - Date.now()) / 86400000);
    if (days === 0) return 'Today';
    if (days === 1) return 'Tomorrow';
    if (days === -1) return 'Yesterday';
    return days > 0 ? `in ${days} days` : `${Math.abs(days)} days overdue`;
  }

  priorityClass(priority: string): string {
    switch (priority) {
      case 'High': return 'bg-rose-100 text-rose-700 dark:bg-rose-900/40 dark:text-rose-300';
      case 'Low': return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
      default: return 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300';
    }
  }
}
