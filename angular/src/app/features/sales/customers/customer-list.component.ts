import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerApiService, CustomerProfile } from '../../../core/services/api/customer-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { StateService } from '../../../core/services/state.service';
import { PERMISSIONS } from '../../../core/models/permissions';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [FormsModule, CommonModule, TranslatePipe],
  templateUrl: './customer-list.component.html'
})
export class CustomerListComponent {
  private customerApi = inject(CustomerApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
  state = inject(StateService);

  readonly PERMISSIONS = PERMISSIONS;

  customers = signal<CustomerProfile[]>([]);
  showModal = signal(false);
  showStatementModal = signal(false);

  selectedCustomer: CustomerProfile | null = null;

  newCust: Partial<CustomerProfile> = {
    name: '', email: '', phone: '', address: '', contactPerson: '',
    taxNumber: '', creditLimit: 100000, paymentTerms: 'Net 30 Days', currency: 'USD', isActive: true
  };

  constructor() {
    this.loadCustomers();
  }

  async loadCustomers() {
    this.customers.set(await this.customerApi.getCustomers());
  }

  openAddModal() {
    this.newCust = {
      customerCode: `CUST-00${this.customers().length + 1}`,
      name: '', email: '', phone: '', address: '', contactPerson: '',
      taxNumber: 'EG-900', creditLimit: 100000, paymentTerms: 'Net 30 Days', currency: 'USD', isActive: true
    };
    this.showModal.set(true);
  }

  async saveCustomer() {
    await this.customerApi.createCustomer(this.newCust);
    this.toast.success('Customer profile registered.');
    this.showModal.set(false);
    await this.loadCustomers();
  }

  viewStatement(cust: CustomerProfile) {
    this.selectedCustomer = cust;
    this.showStatementModal.set(true);
  }
}
