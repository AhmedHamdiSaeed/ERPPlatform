using System;

namespace ERPPlatform.Domain.Events
{
    public class SalesInvoiceCreatedEvent
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; }
    }

    public class PaymentReceivedEvent
    {
        public Guid PaymentId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Bank Transfer";
    }

    public class GoodsReceivedEvent
    {
        public Guid GoodsReceiptId { get; set; }
        public string GrnNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = "Main Warehouse";
        public DateTime ReceivedDate { get; set; }
    }
}
