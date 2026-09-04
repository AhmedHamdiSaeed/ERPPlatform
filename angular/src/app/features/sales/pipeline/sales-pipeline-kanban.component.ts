import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Deal } from '../../../core/models/erp-models';
import { SharedApiService } from '../../../core/services/api/shared-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-sales-pipeline-kanban',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './sales-pipeline-kanban.component.html'
})
export class SalesPipelineKanbanComponent {
  private toast = inject(ToastService);
  private sharedApi = inject(SharedApiService);

  deals = signal<Deal[]>([]);
  stages = ['Prospecting', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost'];

  showAddModal = signal(false);
  newTitle = '';
  newCustomer = '';
  newValue = 50000;
  newStage: 'Prospecting' | 'Proposal' | 'Negotiation' | 'Closed Won' | 'Closed Lost' = 'Prospecting';

  constructor() {
    this.loadDeals();
  }

  async loadDeals() {
    try {
      this.deals.set(await this.sharedApi.getDeals());
    } catch (e) {
      console.error('Failed to load deals', e);
      this.toast.error('Could not load pipeline deals from the server.');
    }
  }

  totalPipelineValue = computed(() =>
    this.deals().reduce((sum, d) => sum + d.value, 0)
  );

  weightedForecast = computed(() =>
    this.deals().reduce((sum, d) => sum + (d.value * (d.probability / 100)), 0)
  );

  getDealsByStage(stage: string) {
    return this.deals().filter(d => d.stage === stage);
  }

  moveDeal(deal: Deal, newStage: 'Prospecting' | 'Proposal' | 'Negotiation' | 'Closed Won' | 'Closed Lost') {
    let prob = deal.probability;
    if (newStage === 'Closed Won') prob = 100;
    else if (newStage === 'Closed Lost') prob = 0;
    else if (newStage === 'Negotiation') prob = 80;
    else if (newStage === 'Proposal') prob = 60;

    // Optimistic UI update
    this.deals.update(list =>
      list.map(d => d.id === deal.id ? { ...d, stage: newStage, probability: prob } : d)
    );
    this.toast.success(`Deal "${deal.title}" moved to ${newStage}.`);

    this.sharedApi.updateDealStage(deal.id, newStage).catch(e => {
      console.error('Failed to persist deal stage', e);
      this.toast.error(`Could not save the new stage for "${deal.title}".`);
    });
  }

  openAddModal() {
    this.newTitle = '';
    this.newCustomer = '';
    this.newValue = 50000;
    this.newStage = 'Prospecting';
    this.showAddModal.set(true);
  }

  async saveDeal() {
    if (!this.newTitle || !this.newCustomer) {
      this.toast.warning('Please enter deal title and customer name.');
      return;
    }

    try {
      await this.sharedApi.createDeal({
        title: this.newTitle,
        customerName: this.newCustomer,
        value: this.newValue,
        stage: this.newStage,
        probability: this.newStage === 'Closed Won' ? 100 : 30,
        ownerName: 'Account Executive'
      });
      await this.loadDeals();
      this.toast.success(`New deal opportunity "${this.newTitle}" created.`);
    } catch (e) {
      console.error('Failed to create deal', e);
      this.toast.error('Failed to create the deal opportunity.');
    }
    this.showAddModal.set(false);
  }
}
