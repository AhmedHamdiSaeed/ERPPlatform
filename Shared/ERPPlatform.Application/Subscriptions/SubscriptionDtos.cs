using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace ERPPlatform.Application.Subscriptions
{
    public class PlanDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string BillingPeriod { get; set; } = "Monthly";
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CreateUpdatePlanDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string BillingPeriod { get; set; } = "Monthly";
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class FeatureDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ValueType { get; set; } = "Integer";
        public string Unit { get; set; } = "count";
        public bool IsActive { get; set; }
    }

    public class CreateUpdateFeatureDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ValueType { get; set; } = "Integer";
        public string Unit { get; set; } = "count";
        public bool IsActive { get; set; } = true;
    }

    public class PlanFeatureDto : EntityDto<Guid>
    {
        public Guid PlanId { get; set; }
        public Guid FeatureId { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public long? LimitValue { get; set; }
        public string LimitType { get; set; } = "Monthly";
        public string Unit { get; set; } = "count";
    }

    public class SetPlanFeatureDto
    {
        public Guid FeatureId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public long? LimitValue { get; set; }
        public string LimitType { get; set; } = "Monthly";
    }

    public class SubscriptionDto : EntityDto<Guid>
    {
        public Guid? TenantId { get; set; }
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = "FREE";
        public string Status { get; set; } = "Active";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public bool AutoRenew { get; set; }
        public decimal PlanPrice { get; set; }
    }

    public class FeatureLimitResultDto
    {
        public bool Allowed { get; set; }
        public long CurrentUsage { get; set; }
        public long? Limit { get; set; }
        public long? Remaining { get; set; }
        public bool IsUnlimited { get; set; }
        public bool FeatureEnabled { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
    }

    public class FeatureAccessDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public long? Limit { get; set; }
        public long Usage { get; set; }
        public long? Remaining { get; set; }
        public string Unit { get; set; } = "count";
        public string Category { get; set; } = "General";
    }
}
