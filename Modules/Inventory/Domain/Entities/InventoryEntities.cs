using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Modules.Inventory.Domain.Entities
{
    public class Product : FullAuditedAggregateRoot<Guid>
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string Unit { get; set; } = "pcs";
        public string WarehouseName { get; set; } = "Main Warehouse";
        public string Status { get; set; } = "In Stock"; // In Stock, Low Stock, Out of Stock
        public string SupplierName { get; set; } = string.Empty;
    }

    public class Warehouse : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public int TotalProductsCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int CapacityPercentage { get; set; }
    }

    public class StockTransfer : FullAuditedAggregateRoot<Guid>
    {
        public string TransferCode { get; set; } = string.Empty;
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "In Transit"; // Pending Approval, In Transit, Completed
    }

    public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
    {
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending Approval"; // Pending Approval, Approved
    }
}
