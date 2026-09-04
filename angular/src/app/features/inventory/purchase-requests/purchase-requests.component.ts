import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PurchaseApiService, PurchaseRequest, RfqItem } from '../../../core/services/api/purchase-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

@Component({
  selector: 'app-purchase-requests',
  standalone: true,
  imports: [FormsModule, CommonModule, TranslatePipe, AppDatePipe],
  templateUrl: './purchase-requests.component.html'
})
export class PurchaseRequestsComponent {
  private purchaseApi = inject(PurchaseApiService);
  private toast = inject(ToastService);

  activeTab = signal<'pr' | 'rfq'>('pr');

  requests = signal<PurchaseRequest[]>([]);
  rfqs = signal<RfqItem[]>([]);

  showPrModal = signal(false);
  showRfqModal = signal(false);

  newPr: Partial<PurchaseRequest> = { departmentName: 'Information Technology', requestedBy: 'Ahmed Hamdi', quantity: 1, estimatedCost: 1000 };
  newRfq: Partial<RfqItem> = this.emptyRfq();

  private emptyRfq(): Partial<RfqItem> {
    const today = new Date();
    const inTwoWeeks = new Date(today.getTime() + 14 * 24 * 60 * 60 * 1000);
    const iso = (d: Date) => d.toISOString().slice(0, 10);
    return { supplierName: 'TechSupply Co.', status: 'Sent', issueDate: iso(today), deadlineDate: iso(inTwoWeeks) };
  }

  constructor() {
    this.loadData();
  }

  async loadData() {
    const [pr, rfq] = await Promise.all([
      this.purchaseApi.getPurchaseRequests(),
      this.purchaseApi.getRfqs()
    ]);
    this.requests.set(pr);
    this.rfqs.set(rfq);
  }

  async savePr() {
    await this.purchaseApi.createPurchaseRequest(this.newPr);
    this.toast.success('Purchase Request submitted for approval.');
    this.showPrModal.set(false);
    await this.loadData();
  }

  async approvePr(id: string) {
    await this.purchaseApi.approvePurchaseRequest(id);
    this.toast.success('Purchase Request approved!');
    await this.loadData();
  }

  async saveRfq() {
    await this.purchaseApi.createRfq(this.newRfq);
    this.toast.success('RFQ dispatched to vendor.');
    this.showRfqModal.set(false);
    this.newRfq = this.emptyRfq();
    await this.loadData();
  }
}
