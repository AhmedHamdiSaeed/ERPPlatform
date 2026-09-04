import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { OrgApiService, Currency, TaxConfig } from '../../../core/services/api/org-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-currency-tax',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './currency-tax.component.html'
})
export class CurrencyTaxComponent {
  private orgApi = inject(OrgApiService);
  private toast = inject(ToastService);

  currencies = signal<Currency[]>([]);
  taxConfigs = signal<TaxConfig[]>([]);

  showCurModal = signal(false);
  showTaxModal = signal(false);

  newCur: Partial<Currency> = { symbol: '$', exchangeRate: 1.0, isActive: true };
  newTax: Partial<TaxConfig> = { taxType: 'VAT', isDefault: false, isActive: true };

  constructor() {
    this.loadData();
  }

  async loadData() {
    const [c, t] = await Promise.all([
      this.orgApi.getCurrencies(),
      this.orgApi.getTaxConfigs()
    ]);
    this.currencies.set(c);
    this.taxConfigs.set(t);
  }

  async saveCurrency() {
    await this.orgApi.createCurrency(this.newCur);
    this.toast.success('Currency configuration added.');
    this.showCurModal.set(false);
    await this.loadData();
  }

  async saveTax() {
    await this.orgApi.createTaxConfig(this.newTax);
    this.toast.success('Tax configuration saved.');
    this.showTaxModal.set(false);
    await this.loadData();
  }
}
