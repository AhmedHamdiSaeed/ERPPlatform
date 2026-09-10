import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CrmApiService, CrmContact } from '../../../core/services/api/crm-api.service';
import { CustomerApiService, CustomerProfile } from '../../../core/services/api/customer-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';

/**
 * CRM contacts — the people behind an account or a lead. Backed by
 * /api/app/crm-contact (CRUD + make-primary).
 */
@Component({
  selector: 'app-crm-contacts',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './contacts.component.html'
})
export class ContactsComponent {
  private crmApi = inject(CrmApiService);
  private customerApi = inject(CustomerApiService);
  private toast = inject(ToastService);
  private dialog = inject(DialogService);

  contacts = signal<CrmContact[]>([]);
  customers = signal<CustomerProfile[]>([]);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);

  draft: Partial<CrmContact> = this.emptyDraft();

  constructor() {
    this.load();
  }

  private emptyDraft(): Partial<CrmContact> {
    return {
      firstName: '',
      lastName: '',
      jobTitle: '',
      email: '',
      phone: '',
      mobile: '',
      customerId: null,
      notes: '',
      isPrimary: false
    };
  }

  async load(): Promise<void> {
    const [contacts, customers] = await Promise.all([
      this.crmApi.getContacts(),
      this.customerApi.getCustomers().catch(() => [] as CustomerProfile[])
    ]);
    this.contacts.set(contacts);
    this.customers.set(customers);
  }

  filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    const list = this.contacts();
    if (!term) return list;
    return list.filter(c =>
      (c.fullName || '').toLowerCase().includes(term) ||
      (c.email || '').toLowerCase().includes(term) ||
      (c.phone || '').toLowerCase().includes(term) ||
      (c.customerName || '').toLowerCase().includes(term) ||
      (c.jobTitle || '').toLowerCase().includes(term)
    );
  });

  accountName(id?: string | null): string {
    if (!id) return '';
    return this.customers().find(c => c.id === id)?.name ?? '';
  }

  openAdd(): void {
    this.editingId.set(null);
    this.draft = this.emptyDraft();
    this.showModal.set(true);
  }

  openEdit(c: CrmContact): void {
    this.editingId.set(c.id);
    this.draft = { ...c };
    this.showModal.set(true);
  }

  async save(): Promise<void> {
    if (!this.draft.firstName?.trim() && !this.draft.lastName?.trim()) {
      this.toast.warning('A contact needs at least a first or last name.');
      return;
    }

    const id = this.editingId();
    if (id) {
      await this.crmApi.updateContact(id, this.draft);
      this.toast.success('Contact updated.');
    } else {
      await this.crmApi.createContact(this.draft);
      this.toast.success('Contact created.');
    }

    this.showModal.set(false);
    await this.load();
  }

  async makePrimary(c: CrmContact): Promise<void> {
    await this.crmApi.makePrimaryContact(c.id);
    this.toast.success(`${c.fullName} is now the primary contact.`);
    await this.load();
  }

  async remove(c: CrmContact): Promise<void> {
    const confirmed = await this.dialog.confirm({
      title: 'Delete Contact',
      message: `Are you sure you want to delete ${c.fullName}?`,
      confirmText: 'Delete',
      type: 'danger'
    });
    if (!confirmed) return;

    await this.crmApi.deleteContact(c.id);
    this.toast.success('Contact removed.');
    await this.load();
  }
}
