using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Finance
{
    // Aging bucket DTO
    public class AgingBucketDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Bucket { get; set; } = "Current"; // Current, 0-30, 31-60, 61-90, 90+
    }

    public class AgingSummaryDto
    {
        public decimal Current { get; set; }
        public decimal Bucket0_30 { get; set; }
        public decimal Bucket31_60 { get; set; }
        public decimal Bucket61_90 { get; set; }
        public decimal Bucket90Plus { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public interface IAccountsReceivableAppService : IApplicationService
    {
        Task<ListResultDto<AgingBucketDto>> GetAgingReportAsync();
        Task<AgingSummaryDto> GetAgingSummaryAsync();
    }

    public class AccountsReceivableAppService : ApplicationService, IAccountsReceivableAppService
    {
        private readonly IRepository<SalesInvoice, Guid> _invoiceRepository;

        public AccountsReceivableAppService(IRepository<SalesInvoice, Guid> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<ListResultDto<AgingBucketDto>> GetAgingReportAsync()
        {
            var invoices = await _invoiceRepository.GetListAsync();
            var now = DateTime.UtcNow;
            var unpaid = invoices.Where(i => i.Status != "Paid" && i.Status != "Cancelled").ToList();

            var result = unpaid.Select(i =>
            {
                var daysOverdue = (int)(now - i.DueDate).TotalDays;
                var bucket = daysOverdue <= 0 ? "Current"
                    : daysOverdue <= 30 ? "0-30"
                    : daysOverdue <= 60 ? "31-60"
                    : daysOverdue <= 90 ? "61-90"
                    : "90+";
                return new AgingBucketDto
                {
                    CustomerName = i.CustomerName,
                    InvoiceNumber = i.InvoiceNumber,
                    Amount = i.TotalAmount,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    DaysOverdue = Math.Max(0, daysOverdue),
                    Bucket = bucket
                };
            }).OrderByDescending(r => r.DaysOverdue).ToList();

            return new ListResultDto<AgingBucketDto>(result);
        }

        public async Task<AgingSummaryDto> GetAgingSummaryAsync()
        {
            var report = await GetAgingReportAsync();
            var summary = new AgingSummaryDto();
            foreach (var item in report.Items)
            {
                summary.TotalOutstanding += item.Amount;
                switch (item.Bucket)
                {
                    case "Current": summary.Current += item.Amount; break;
                    case "0-30": summary.Bucket0_30 += item.Amount; break;
                    case "31-60": summary.Bucket31_60 += item.Amount; break;
                    case "61-90": summary.Bucket61_90 += item.Amount; break;
                    case "90+": summary.Bucket90Plus += item.Amount; break;
                }
            }
            return summary;
        }
    }

    // Accounts Payable aging (for supplier invoices — uses PurchaseOrder as proxy)
    public class ApAgingBucketDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public string PoNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Bucket { get; set; } = "Current";
    }

    public interface IAccountsPayableAppService : IApplicationService
    {
        Task<ListResultDto<ApAgingBucketDto>> GetAgingReportAsync();
    }

    public class AccountsPayableAppService : ApplicationService, IAccountsPayableAppService
    {
        private readonly IRepository<PurchaseOrder, Guid> _poRepository;

        public AccountsPayableAppService(IRepository<PurchaseOrder, Guid> poRepository)
        {
            _poRepository = poRepository;
        }

        public async Task<ListResultDto<ApAgingBucketDto>> GetAgingReportAsync()
        {
            var pos = await _poRepository.GetListAsync();
            var now = DateTime.UtcNow;
            var unpaid = pos.Where(p => p.Status != "Cancelled" && p.Status != "Received").ToList();

            var result = unpaid.Select(p =>
            {
                var daysOverdue = (int)(now - p.DeliveryDate).TotalDays;
                var bucket = daysOverdue <= 0 ? "Current"
                    : daysOverdue <= 30 ? "0-30"
                    : daysOverdue <= 60 ? "31-60"
                    : daysOverdue <= 90 ? "61-90"
                    : "90+";
                return new ApAgingBucketDto
                {
                    SupplierName = p.SupplierName,
                    PoNumber = p.PoNumber,
                    Amount = p.GrandTotal,
                    OrderDate = p.OrderDate,
                    DeliveryDate = p.DeliveryDate,
                    DaysOverdue = Math.Max(0, daysOverdue),
                    Bucket = bucket
                };
            }).OrderByDescending(r => r.DaysOverdue).ToList();

            return new ListResultDto<ApAgingBucketDto>(result);
        }
    }

    // Bank Reconciliation
    public class BankReconciliationDto
    {
        public Guid Id { get; set; }
        public string StatementReference { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal SystemBalance { get; set; }
        public decimal Difference { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Matched, Reconciled
        public string Notes { get; set; } = string.Empty;
    }

    public interface IBankReconciliationAppService : IApplicationService
    {
        Task<BankReconciliationDto> ImportStatementAsync(string statementReference, DateTime statementDate, decimal openingBalance, decimal closingBalance);
        Task<BankReconciliationDto> ReconcileAsync(Guid id);
        Task<ListResultDto<BankReconciliationDto>> GetListAsync();
    }

    public class BankReconciliationAppService : ApplicationService, IBankReconciliationAppService
    {
        private readonly IRepository<JournalEntry, Guid> _journalRepo;
        private static readonly List<BankReconciliationDto> _reconciliations = new();

        public BankReconciliationAppService(IRepository<JournalEntry, Guid> journalRepo)
        {
            _journalRepo = journalRepo;
        }

        public Task<BankReconciliationDto> ImportStatementAsync(string statementReference, DateTime statementDate, decimal openingBalance, decimal closingBalance)
        {
            var dto = new BankReconciliationDto
            {
                Id = Guid.NewGuid(),
                StatementReference = statementReference,
                StatementDate = statementDate,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                SystemBalance = openingBalance, // Will be computed from journals
                Difference = closingBalance - openingBalance,
                Status = "Pending"
            };
            _reconciliations.Add(dto);
            return Task.FromResult(dto);
        }

        public async Task<BankReconciliationDto> ReconcileAsync(Guid id)
        {
            var rec = _reconciliations.FirstOrDefault(r => r.Id == id);
            if (rec == null) throw new ArgumentException("Reconciliation not found");

            var journals = await _journalRepo.GetListAsync();
            var totalCredit = journals.Sum(j => j.TotalCredit);
            var totalDebit = journals.Sum(j => j.TotalDebit);

            rec.SystemBalance = rec.OpeningBalance + totalDebit - totalCredit;
            rec.Difference = rec.ClosingBalance - rec.SystemBalance;
            rec.Status = rec.Difference == 0 ? "Reconciled" : "Matched";

            return rec;
        }

        public Task<ListResultDto<BankReconciliationDto>> GetListAsync()
        {
            return Task.FromResult(new ListResultDto<BankReconciliationDto>(_reconciliations));
        }
    }
}
