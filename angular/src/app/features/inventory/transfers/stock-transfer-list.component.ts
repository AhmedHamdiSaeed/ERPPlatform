import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StockTransfer } from '../../../core/models/erp-models';
import { MOCK_STOCK_TRANSFERS, MOCK_PRODUCTS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">Stock Transfers & Dispatch</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">Track inter-warehouse item movements, approvals, dispatching, and transit statuses.</p>
        </div>

        <button (click)="openTransferModal()" class="btn-primary text-xs cursor-pointer">
          <i class="pi pi-sync"></i> Initiate Stock Transfer
        </button>
      </div>

      <!-- Transfers Registry Table -->
      <div class="card-panel !p-0 overflow-hidden">
        <div class="p-3.5 border-b border-[var(--border-color)] flex items-center justify-between">
          <h3 class="text-xs font-bold text-[var(--text-main)]">Stock Movement History</h3>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead>
              <tr class="bg-slate-100/70 dark:bg-slate-800/70 text-slate-500 text-[11px] font-bold uppercase">
                <th class="p-3.5">Transfer Code</th>
                <th class="p-3.5">Source Warehouse</th>
                <th class="p-3.5">Destination Warehouse</th>
                <th class="p-3.5">Product & Quantity</th>
                <th class="p-3.5">Requested By</th>
                <th class="p-3.5">Date</th>
                <th class="p-3.5">Status</th>
                <th class="p-3.5 text-right">Action</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border-color)] text-[var(--text-main)]">
              @for (st of transfers(); track st.id) {
                <tr class="hover:bg-slate-50/80 dark:hover:bg-slate-800/50">
                  <td class="p-3.5 font-mono text-[11px] font-bold text-blue-600">{{ st.transferCode }}</td>
                  <td class="p-3.5 font-medium text-slate-700 dark:text-slate-300">{{ st.sourceWarehouse }}</td>
                  <td class="p-3.5 font-medium text-slate-700 dark:text-slate-300">{{ st.destinationWarehouse }}</td>
                  <td class="p-3.5 font-bold">{{ st.productName }} <span class="text-blue-600">({{ st.quantity }} pcs)</span></td>
                  <td class="p-3.5 text-slate-500">{{ st.requestedBy }}</td>
                  <td class="p-3.5 text-slate-500">{{ st.date }}</td>
                  <td class="p-3.5">
                    <span class="status-badge" [class.approved]="st.status === 'Approved' || st.status === 'Completed'" [class.pending]="st.status === 'Pending Approval' || st.status === 'In Transit'">
                      {{ st.status }}
                    </span>
                  </td>
                  <td class="p-3.5 text-right">
                    @if (st.status === 'In Transit') {
                      <button (click)="completeTransfer(st.id)" class="px-2.5 py-1 bg-emerald-600 text-white font-bold rounded text-[11px]">Confirm Received</button>
                    }
                    @if (st.status !== 'In Transit') {
                      <span class="text-[11px] text-slate-400 font-medium">Verified</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Initiate Transfer Modal -->
      @if (showModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-fade-in">
          <div class="w-full max-w-md bg-[var(--bg-card)] border border-[var(--border-color)] rounded-2xl shadow-2xl p-6 space-y-4">
            <div class="flex items-center justify-between border-b border-[var(--border-color)] pb-3">
              <h3 class="font-bold text-sm text-[var(--text-main)]">New Inter-Warehouse Transfer</h3>
              <button (click)="showModal.set(false)" class="text-slate-400 hover:text-slate-600"><i class="pi pi-times"></i></button>
            </div>

            <div class="space-y-3 text-xs">
              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Source Warehouse</label>
                <select [(ngModel)]="newTrf.sourceWarehouse" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg">
                  <option value="Main Warehouse">Main Warehouse</option>
                  <option value="Secondary Warehouse">Secondary Warehouse</option>
                  <option value="Central Logistics Hub">Central Logistics Hub</option>
                </select>
              </div>

              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Destination Warehouse</label>
                <select [(ngModel)]="newTrf.destinationWarehouse" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg">
                  <option value="Secondary Warehouse">Secondary Warehouse</option>
                  <option value="Main Warehouse">Main Warehouse</option>
                  <option value="Central Logistics Hub">Central Logistics Hub</option>
                </select>
              </div>

              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Select Product</label>
                <select [(ngModel)]="newTrf.productName" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg">
                  @for (p of products; track p.id) {
                    <option [value]="p.name">{{ p.name }} (Current: {{ p.stock }} pcs)</option>
                  }
                </select>
              </div>

              <div>
                <label class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Transfer Quantity</label>
                <input type="number" [(ngModel)]="newTrf.quantity" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
              </div>
            </div>

            <div class="pt-3 border-t border-[var(--border-color)] flex justify-end gap-2">
              <button (click)="showModal.set(false)" class="btn-outline text-xs">Cancel</button>
              <button (click)="submitTransfer()" class="btn-primary text-xs">Dispatch Transfer</button>
            </div>
          </div>
        </div>
      }

    </div>
  `
})
export class StockTransferListComponent {
  transfers = signal<StockTransfer[]>(MOCK_STOCK_TRANSFERS);
  products = MOCK_PRODUCTS;

  showModal = signal(false);

  newTrf: Partial<StockTransfer> = {
    sourceWarehouse: 'Main Warehouse',
    destinationWarehouse: 'Secondary Warehouse',
    productName: MOCK_PRODUCTS[0].name,
    quantity: 10
  };

  openTransferModal() {
    this.showModal.set(true);
  }

  submitTransfer() {
    const item: StockTransfer = {
      id: `st-${Date.now()}`,
      transferCode: `TRF-2026-00${Math.floor(10 + Math.random()*90)}`,
      sourceWarehouse: this.newTrf.sourceWarehouse!,
      destinationWarehouse: this.newTrf.destinationWarehouse!,
      productName: this.newTrf.productName!,
      quantity: this.newTrf.quantity || 10,
      requestedBy: 'Omar Farouk',
      date: new Date().toISOString().split('T')[0],
      status: 'Pending Approval'
    };
    this.transfers.update(list => [item, ...list]);
    this.showModal.set(false);
  }

  completeTransfer(id: string) {
    this.transfers.update(list => list.map(t => t.id === id ? { ...t, status: 'Completed' } as StockTransfer : t));
  }
}
