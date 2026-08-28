using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace ERPPlatform.Domain.Entities
{
    public class Company : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public string Currency { get; set; } = "USD";
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class Branch : FullAuditedAggregateRoot<Guid>
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsHeadquarters { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class CostCenter : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class FiscalYear : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public bool IsClosed { get; set; } = false;
    }

    public class Currency : FullAuditedAggregateRoot<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = "$";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public bool IsBase { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class TaxConfig : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class PaymentTerm : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public int DueDays { get; set; } = 30;
        public int DiscountDays { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0.0m;
        public string Description { get; set; } = string.Empty;
    }
}
