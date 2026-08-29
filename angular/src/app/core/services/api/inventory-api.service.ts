import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { Product, Warehouse, StockTransfer, PurchaseOrder } from '../../models/erp-models';
import { environment } from '../../../../environments/environment';

interface ProductDto extends AbpEntity {
  sku: string; name: string; category: string; price: number; stock: number;
  reorderLevel: number; unit: string; warehouseName: string; status: string; supplierName: string;
}

interface WarehouseDto extends AbpEntity {
  code: string; name: string; location: string; manager: string;
  totalProductsCount: number; totalStockValue: number; capacityPercentage: number;
}

interface StockTransferDto extends AbpEntity {
  transferCode: string; sourceWarehouse: string; destinationWarehouse: string;
  productName: string; quantity: number; requestedBy: string; date: string; status: string;
}

interface PurchaseOrderDto extends AbpEntity {
  poNumber: string; supplierName: string; orderDate: string; deliveryDate: string;
  items: unknown[]; subtotal: number; tax: number; discount: number;
  grandTotal: number; createdBy: string; status: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/inventory`;
  }

  getProducts(): Promise<Product[]> {
    return this.getList<ProductDto>('product').then(items =>
      items.map(p => ({
        id: p.id, sku: p.sku, name: p.name, category: p.category, price: p.price,
        stock: p.stock, reorderLevel: p.reorderLevel, unit: p.unit,
        warehouseName: p.warehouseName, status: p.status, supplierName: p.supplierName
      })) as Product[]
    );
  }

  createProduct(product: Partial<Product>): Promise<void> {
    return this.post('product', product);
  }

  updateProduct(id: string, product: Partial<Product>): Promise<void> {
    return this.put(`product/${id}`, product);
  }

  deleteProduct(id: string): Promise<void> {
    return this.delete(`product/${id}`);
  }

  adjustStock(id: string, newStock: number): Promise<void> {
    return this.post(`product/${id}/adjust-stock?newStock=${newStock}`, {});
  }

  getWarehouses(): Promise<Warehouse[]> {
    return this.getList<WarehouseDto>('warehouse').then(items => items as Warehouse[]);
  }

  createWarehouse(wh: Partial<Warehouse>): Promise<void> {
    return this.post('warehouse', wh);
  }

  updateWarehouse(id: string, wh: Partial<Warehouse>): Promise<void> {
    return this.put(`warehouse/${id}`, wh);
  }

  deleteWarehouse(id: string): Promise<void> {
    return this.delete(`warehouse/${id}`);
  }

  getStockTransfers(): Promise<StockTransfer[]> {
    return this.getList<StockTransferDto>('stock-transfer').then(items =>
      items.map(t => ({ ...t, date: toDateString(t.date) })) as StockTransfer[]
    );
  }

  createStockTransfer(transfer: Partial<StockTransfer>): Promise<void> {
    return this.post('stock-transfer', transfer);
  }

  updateStockTransferStatus(id: string, newStatus: string): Promise<void> {
    return this.put(`stock-transfer/${id}/status?newStatus=${encodeURIComponent(newStatus)}`, {});
  }

  deleteStockTransfer(id: string): Promise<void> {
    return this.delete(`stock-transfer/${id}`);
  }

  getPurchaseOrders(): Promise<PurchaseOrder[]> {
    return this.getList<PurchaseOrderDto>('purchase-order').then(items =>
      items.map(o => ({
        ...o,
        items: [],
        orderDate: toDateString(o.orderDate),
        deliveryDate: toDateString(o.deliveryDate)
      })) as PurchaseOrder[]
    );
  }

  createPurchaseOrder(order: Partial<PurchaseOrder>): Promise<void> {
    const body = {
      poNumber: order.poNumber,
      supplierName: order.supplierName,
      deliveryDate: order.deliveryDate,
      discount: order.discount ?? 0,
      createdBy: order.createdBy,
      status: order.status
    };
    return this.post('purchase-order', body);
  }

  updatePurchaseOrderStatus(id: string, newStatus: string): Promise<void> {
    return this.put(`purchase-order/${id}/status?newStatus=${encodeURIComponent(newStatus)}`, {});
  }

  deletePurchaseOrder(id: string): Promise<void> {
    return this.delete(`purchase-order/${id}`);
  }
}
