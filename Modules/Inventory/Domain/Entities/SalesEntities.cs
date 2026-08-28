using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Domain.Entities
{
    public class SalesInvoice : FullAuditedAggregateRoot<Guid>
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class SalesQuotation : FullAuditedAggregateRoot<Guid>
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Rejected, Expired
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
