import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { SubscriptionApiService, PlanDto, PlanFeatureDto } from '../../../core/services/api/subscription-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-plans',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './plans.component.html'
})
export class PlansComponent {
  private subscriptionApi = inject(SubscriptionApiService);
  private toast = inject(ToastService);

  plans = signal<PlanDto[]>([]);
  selectedPlan = signal<PlanDto | null>(null);
  planFeatures = signal<PlanFeatureDto[]>([]);
  showConfigModal = signal(false);

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.plans.set(await this.subscriptionApi.getPlans());
  }

  async configureFeatures(plan: PlanDto) {
    this.selectedPlan.set(plan);
    const feats = await this.subscriptionApi.getPlanFeatures(plan.id);
    this.planFeatures.set(feats);
    this.showConfigModal.set(true);
  }

  async saveFeatures() {
    if (this.selectedPlan()) {
      await this.subscriptionApi.updatePlanFeatures(this.selectedPlan()!.id, this.planFeatures());
      this.toast.success(`Feature limits updated for ${this.selectedPlan()!.name}`);
      this.showConfigModal.set(false);
    }
  }
}
