import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PurchaseApiService, GoodsReceiptItem } from '../../../core/services/api/purchase-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-goods-receipts',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './goods-receipts.component.html'
})
export class GoodsReceiptsComponent {
  private purchaseApi = inject(PurchaseApiService);
  private toast = inject(ToastService);

  receipts = signal<GoodsReceiptItem[]>([]);

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.receipts.set(await this.purchaseApi.getGoodsReceipts());
  }

  async passQc(id: string) {
    await this.purchaseApi.passQualityCheck(id);
    this.toast.success('Quality Check Passed! Stock level incremented in target warehouse.');
    await this.loadData();
  }
}
