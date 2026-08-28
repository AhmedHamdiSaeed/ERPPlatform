import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionApiService, FeatureAccessDto } from '../../../core/services/api/subscription-api.service';

@Component({
  selector: 'app-subscription-usage',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription-usage.component.html'
})
export class SubscriptionUsageComponent {
  private subscriptionApi = inject(SubscriptionApiService);

  features = signal<FeatureAccessDto[]>([]);
  showUpgradeModal = signal(false);

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.features.set(await this.subscriptionApi.getSubscriptionFeatures());
  }

  getPercentage(f: FeatureAccessDto): number {
    if (!f.limit || f.limit === 0) return 0;
    return Math.min(100, Math.round((f.usage / f.limit) * 100));
  }

  getThresholdClass(pct: number): string {
    if (pct >= 100) return 'bg-rose-600';
    if (pct >= 90) return 'bg-amber-500';
    if (pct >= 70) return 'bg-amber-400';
    return 'bg-blue-600';
  }
}
