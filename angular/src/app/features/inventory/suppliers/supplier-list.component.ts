import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupplierApiService, SupplierProfile } from '../../../core/services/api/supplier-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [FormsModule, CommonModule, TranslatePipe],
  templateUrl: './supplier-list.component.html'
})
export class SupplierListComponent {
  private supplierApi = inject(SupplierApiService);
  private toast = inject(ToastService);

  suppliers = signal<SupplierProfile[]>([]);
  showModal = signal(false);

  newSup: Partial<SupplierProfile> = {
    companyName: '', email: '', phone: '', address: '', contactPerson: '',
    taxNumber: '', creditLimit: 200000, paymentTerms: 'Net 30 Days', isActive: true
  };

  constructor() {
    this.loadSuppliers();
  }

  async loadSuppliers() {
    this.suppliers.set(await this.supplierApi.getSuppliers());
  }

  openAddModal() {
    this.newSup = {
      supplierCode: `SUP-00${this.suppliers().length + 1}`,
      companyName: '', email: '', phone: '', address: '', contactPerson: '',
      taxNumber: 'US-800', creditLimit: 200000, paymentTerms: 'Net 30 Days', isActive: true
    };
    this.showModal.set(true);
  }

  async saveSupplier() {
    await this.supplierApi.createSupplier(this.newSup);
    this.toast.success('Supplier profile created.');
    this.showModal.set(false);
    await this.loadSuppliers();
  }
}
