import { Component, inject, signal, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../core/services/state.service';
import { AiService } from '../../core/services/ai.service';
import { TranslationService } from '../../core/services/translation.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { DashboardApiService, DashboardStats } from '../../core/services/api/dashboard-api.service';
import { ChartModule } from 'primeng/chart';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ChartModule, TranslatePipe],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  state = inject(StateService);
  aiService = inject(AiService);
  private translation = inject(TranslationService);
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

  // Department Bar Chart Data using translation keys
  deptChartData = computed(() => {
    return {
      labels: [
        this.translation.get('Dept:IT'),
        this.translation.get('Dept:HR'),
        this.translation.get('Dept:Finance'),
        this.translation.get('Dept:Sales'),
        this.translation.get('Dept:Logistics'),
        this.translation.get('Dept:QA'),
        this.translation.get('Dept:Support')
      ],
      datasets: [
        {
          label: this.translation.get('Dept:EmployeeCount'),
          data: [42, 18, 24, 56, 65, 15, 25],
          backgroundColor: '#2563eb',
          borderRadius: 6
        }
      ]
    };
  });

  // Workflow Doughnut Chart Data using translation keys
  workflowChartData = computed(() => {
    return {
      labels: [
        this.translation.get('Workflow:Completed'),
        this.translation.get('Workflow:RunningPending'),
        this.translation.get('Workflow:Failed')
      ],
      datasets: [
        {
          data: [82, 15, 3],
          backgroundColor: ['#10b981', '#f59e0b', '#ef4444']
        }
      ]
    };
  });

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

  activities = computed(() => {
    const isAr = this.state.isRtl();
    return [
      {
        user: isAr ? 'أحمد حمدي' : 'Ahmed Hamdi',
        action: isAr ? 'أنشأ أمر شراء' : 'created purchase order',
        detail: isAr ? 'PO-2026-8801 لشركة توريد التقنية (18,880 دولار)' : 'PO-2026-8801 for TechSupply Co. ($18,880)',
        time: isAr ? 'منذ 10 دقائق' : '10 mins ago',
        icon: 'pi-file',
        bg: 'bg-blue-100 text-blue-600'
      },
      {
        user: isAr ? 'سارة محمود' : 'Sara Mahmoud',
        action: isAr ? 'وافقت على طلب إجازة لـ' : 'approved leave request for',
        detail: isAr ? 'منى زكي (يومان إجازة مرضية)' : 'Mona Zaki (2 days Sick Leave)',
        time: isAr ? 'منذ 25 دقيقة' : '25 mins ago',
        icon: 'pi-check',
        bg: 'bg-emerald-100 text-emerald-600'
      },
      {
        user: isAr ? 'حارس النظام' : 'System Sentinel',
        action: isAr ? 'اكتشف تنبيه انخفاض المخزون على' : 'detected low stock alert on',
        detail: isAr ? 'ماوس Logitech MX Master 3S (متبقي 8)' : 'Logitech MX Master 3S Mouse (8 left)',
        time: isAr ? 'منذ ساعة واحدة' : '1 hour ago',
        icon: 'pi-exclamation-triangle',
        bg: 'bg-amber-100 text-amber-600'
      },
      {
        user: isAr ? 'عمر فاروق' : 'Omar Farouk',
        action: isAr ? 'بدأ تحويل المخزون' : 'initiated stock transfer',
        detail: isAr ? 'TRF-2026-002 من المركز الرئيسي إلى المستودع العام' : 'TRF-2026-002 from Central Hub to Main WH',
        time: isAr ? 'منذ 3 ساعات' : '3 hours ago',
        icon: 'pi-sync',
        bg: 'bg-indigo-100 text-indigo-600'
      }
    ];
  });

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
