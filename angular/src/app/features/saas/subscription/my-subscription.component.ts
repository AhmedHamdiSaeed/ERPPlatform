import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionApiService, SubscriptionDto, PlanDto } from '../../../core/services/api/subscription-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

@Component({
  selector: 'app-my-subscription',
  standalone: true,
  imports: [CommonModule, TranslatePipe, AppDatePipe],
  templateUrl: './my-subscription.component.html'
})
export class MySubscriptionComponent {
  private subscriptionApi = inject(SubscriptionApiService);
  private toast = inject(ToastService);

  sub = signal<SubscriptionDto | null>(null);
  plans = signal<PlanDto[]>([]);
  showUpgradeModal = signal(false);

  constructor() {
    this.loadData();
  }

  async loadData() {
    const [subData, plansData] = await Promise.all([
      this.subscriptionApi.getCurrentSubscription(),
      this.subscriptionApi.getPlans()
    ]);
    this.sub.set(subData);
    this.plans.set(plansData);
  }

  async selectPlan(planId: string) {
    const updated = await this.subscriptionApi.changePlan(planId);
    this.sub.set(updated);
    this.toast.success(`Subscription upgraded to ${updated.planName}!`);
    this.showUpgradeModal.set(false);
  }

  async cancel() {
    await this.subscriptionApi.cancelSubscription();
    this.toast.info('Subscription cancelled. Access remains active until end of billing cycle.');
    await this.loadData();
  }

  async resume() {
    await this.subscriptionApi.resumeSubscription();
    this.toast.success('Subscription resumed.');
    await this.loadData();
  }
}
