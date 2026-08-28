import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrgApiService, PaymentTerm } from '../../../core/services/api/org-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-payment-terms',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './payment-terms.component.html'
})
export class PaymentTermsComponent {
  private orgApi = inject(OrgApiService);
  private toast = inject(ToastService);

  paymentTerms = signal<PaymentTerm[]>([]);
  showModal = signal(false);
  newTerm: Partial<PaymentTerm> = { dueDays: 30, discountDays: 0, discountPercent: 0 };

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.paymentTerms.set(await this.orgApi.getPaymentTerms());
  }

  async saveTerm() {
    await this.orgApi.createPaymentTerm(this.newTerm);
    this.toast.success('Payment term configuration saved.');
    this.showModal.set(false);
    await this.loadData();
  }
}
