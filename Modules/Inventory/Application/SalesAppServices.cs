using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Subscriptions;
using ERPPlatform.Application.Subscriptions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class SalesInvoiceDto : EntityDto<Guid>
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class SalesQuotationDto : EntityDto<Guid>
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class SalesDashboardStatsDto
    {
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class SalesInvoiceAppService : CrudAppService<SalesInvoice, SalesInvoiceDto, Guid, PagedAndSortedResultRequestDto, SalesInvoiceDto>
    {
        private readonly IFeatureChecker _featureChecker;
        private readonly IUsageService _usageService;

        public SalesInvoiceAppService(
            IRepository<SalesInvoice, Guid> repository,
            IFeatureChecker featureChecker,
            IUsageService usageService) : base(repository)
        {
            _featureChecker = featureChecker;
            _usageService = usageService;
        }

        public override async Task<SalesInvoiceDto> CreateAsync(SalesInvoiceDto input)
        {
            // 1. Subscription Feature Limit Check
            var check = await _featureChecker.CheckLimitAsync(ErpFeatures.InvoicesMonthly, 1);
            if (!check.Allowed)
            {
                throw new UserFriendlyException($"Monthly invoice limit of {check.Limit} reached! Upgrade your SaaS subscription plan to create more invoices.");
            }

            var result = await base.CreateAsync(input);

            // 2. Increment usage counter
            await _usageService.IncrementAsync(CurrentTenant.Id, ErpFeatures.InvoicesMonthly, 1);
            return result;
        }

        public async Task MarkAsPaidAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            entity.Status = "Paid";
            await Repository.UpdateAsync(entity);
        }

        public async Task SendInvoiceAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            entity.Status = "Sent";
            await Repository.UpdateAsync(entity);
        }

        public async Task<SalesDashboardStatsDto> GetStatsAsync()
        {
            var list = await Repository.GetListAsync();
            return new SalesDashboardStatsDto
            {
                TotalInvoices = list.Count,
                PaidInvoices = list.Count(i => i.Status == "Paid"),
                PendingAmount = list.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount),
                PaidAmount = list.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount)
            };
        }
    }

    public class SalesQuotationAppService : CrudAppService<SalesQuotation, SalesQuotationDto, Guid, PagedAndSortedResultRequestDto, SalesQuotationDto>
    {
        private readonly IRepository<SalesInvoice, Guid> _invoiceRepository;
        private readonly IFeatureChecker _featureChecker;
        private readonly IUsageService _usageService;

        public SalesQuotationAppService(
            IRepository<SalesQuotation, Guid> repository,
            IRepository<SalesInvoice, Guid> invoiceRepository,
            IFeatureChecker featureChecker,
            IUsageService usageService) : base(repository)
        {
            _invoiceRepository = invoiceRepository;
            _featureChecker = featureChecker;
            _usageService = usageService;
        }

        public override async Task<SalesQuotationDto> CreateAsync(SalesQuotationDto input)
        {
            var check = await _featureChecker.CheckLimitAsync(ErpFeatures.QuotationsMonthly, 1);
            if (!check.Allowed)
            {
                throw new UserFriendlyException($"Monthly quotation limit of {check.Limit} reached! Upgrade your SaaS subscription plan to create more quotations.");
            }

            var result = await base.CreateAsync(input);
            await _usageService.IncrementAsync(CurrentTenant.Id, ErpFeatures.QuotationsMonthly, 1);
            return result;
        }

        public async Task ConvertToInvoiceAsync(Guid quotationId)
        {
            var q = await Repository.GetAsync(quotationId);
            q.Status = "Accepted";
            await Repository.UpdateAsync(q);

            var invoice = new SalesInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}",
                CustomerName = q.CustomerName,
                CustomerEmail = q.CustomerEmail,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                Subtotal = q.Subtotal,
                TaxAmount = q.TaxAmount,
                Discount = q.Discount,
                TotalAmount = q.TotalAmount,
                Status = "Sent",
                Notes = $"Converted from Quotation {q.QuotationNumber}",
                CreatedBy = q.CreatedBy
            };
            await _invoiceRepository.InsertAsync(invoice);
            await _usageService.IncrementAsync(CurrentTenant.Id, ErpFeatures.InvoicesMonthly, 1);
        }
    }
}
