using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    // Existing DTOs
    public class CompanyDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string TaxNumber { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string Country { get; set; } = "Egypt"; public string Currency { get; set; } = "USD"; public string Website { get; set; } = string.Empty; public string LogoUrl { get; set; } = string.Empty; public string PrimaryColor { get; set; } = "#1890ff"; public string SecondaryColor { get; set; } = "#52c41a"; public string Theme { get; set; } = "default"; public bool IsActive { get; set; } = true; }
    public class BranchDto : EntityDto<Guid> { public Guid CompanyId { get; set; } public string CompanyName { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public bool IsHeadquarters { get; set; } public bool IsActive { get; set; } }
    public class CostCenterDto : EntityDto<Guid> { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class FiscalYearDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public bool IsCurrent { get; set; } public bool IsClosed { get; set; } }
    public class CurrencyDto : EntityDto<Guid> { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Symbol { get; set; } = "$"; public decimal ExchangeRate { get; set; } public bool IsBase { get; set; } public bool IsActive { get; set; } }
    public class TaxConfigDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public decimal Rate { get; set; } public string TaxType { get; set; } = "VAT"; public bool IsDefault { get; set; } public bool IsActive { get; set; } }
    public class PaymentTermDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public int DueDays { get; set; } public int DiscountDays { get; set; } public decimal DiscountPercent { get; set; } public string Description { get; set; } = string.Empty; }
    public class LeadDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Source { get; set; } = "Website";
        public string Status { get; set; } = "New";
        public string SalespersonName { get; set; } = string.Empty;
        public DateTime NextFollowUp { get; set; }
        public string Notes { get; set; } = string.Empty;

        // ── Qualification & conversion ─────────────────────────────────
        public Guid? OwnerUserId { get; set; }
        public int Score { get; set; }
        public string LostReason { get; set; } = string.Empty;
        public DateTime? QualifiedAt { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public Guid? ConvertedCustomerId { get; set; }
        public Guid? ConvertedDealId { get; set; }
        public string Tags { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }

    public class CustomerDto : EntityDto<Guid>
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string PaymentTerms { get; set; } = "Net 30 Days";
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; } = true;

        // ── Account enrichment ─────────────────────────────────────────
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public Guid? OwnerUserId { get; set; }
        public bool IsVip { get; set; }
        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>Search & filter input for the leads board.</summary>
    public class LeadListInput : PagedAndSortedResultRequestDto
    {
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? Owner { get; set; }
        /// <summary>Free-text match on name, company, email or phone.</summary>
        public string? Filter { get; set; }
        /// <summary>Only leads whose NextFollowUp is in the past.</summary>
        public bool? OnlyOverdueFollowUp { get; set; }
    }

    public class LeadLostInput
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class AssignLeadInput
    {
        public string SalespersonName { get; set; } = string.Empty;
        public Guid? OwnerUserId { get; set; }
    }
    public class SupplierDto : EntityDto<Guid> { public string SupplierCode { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string ContactPerson { get; set; } = string.Empty; public string TaxNumber { get; set; } = string.Empty; public decimal CreditLimit { get; set; } public decimal OutstandingBalance { get; set; } public string PaymentTerms { get; set; } = "Net 30 Days"; public bool IsActive { get; set; } = true; }
    public class SalesOrderDto : EntityDto<Guid> { public string OrderNumber { get; set; } = string.Empty; public string CustomerName { get; set; } = string.Empty; public DateTime OrderDate { get; set; } public DateTime ExpectedDeliveryDate { get; set; } public decimal Subtotal { get; set; } public decimal TaxAmount { get; set; } public decimal TotalAmount { get; set; } public string Status { get; set; } = "Draft"; public string Notes { get; set; } = string.Empty; }
    public class DeliveryNoteDto : EntityDto<Guid> { public string DeliveryNumber { get; set; } = string.Empty; public string SalesOrderNumber { get; set; } = string.Empty; public string CustomerName { get; set; } = string.Empty; public DateTime DeliveryDate { get; set; } public string DispatchWarehouse { get; set; } = "Main Warehouse"; public string Status { get; set; } = "Dispatched"; public string Carrier { get; set; } = "Express Freight"; public string TrackingNumber { get; set; } = string.Empty; public string CurrentLocation { get; set; } = string.Empty; public string DeliveryProof { get; set; } = string.Empty; public string SignatureBase64 { get; set; } = string.Empty; public string SignedBy { get; set; } = string.Empty; public DateTime? SignedAt { get; set; } }
    public class PurchaseRequestDto : EntityDto<Guid> { public string PrNumber { get; set; } = string.Empty; public string DepartmentName { get; set; } = string.Empty; public string RequestedBy { get; set; } = string.Empty; public string ItemName { get; set; } = string.Empty; public int Quantity { get; set; } public decimal EstimatedCost { get; set; } public string Status { get; set; } = "Pending Approval"; }
    public class RfqDto : EntityDto<Guid> { public string RfqNumber { get; set; } = string.Empty; public string SupplierName { get; set; } = string.Empty; public string Title { get; set; } = string.Empty; public DateTime IssueDate { get; set; } public DateTime DeadlineDate { get; set; } public string Status { get; set; } = "Sent"; }
    public class GoodsReceiptDto : EntityDto<Guid> { public string GrnNumber { get; set; } = string.Empty; public string PoNumber { get; set; } = string.Empty; public string SupplierName { get; set; } = string.Empty; public DateTime ReceivedDate { get; set; } public string ReceivingWarehouse { get; set; } = "Main Warehouse"; public string QcStatus { get; set; } = "Passed"; }

    // DTOs for Modules 16-20
    public class ExpenseRequestDto : EntityDto<Guid>
    {
        public string ExpenseCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Category { get; set; } = "Travel";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Pending Approval";
    }

    public class ProjectDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal SpentAmount { get; set; }
        public int ProgressPercentage { get; set; }
        public string Status { get; set; } = "In Progress";
        public DateTime Deadline { get; set; }
    }

    public class BillOfMaterialsDto : EntityDto<Guid>
    {
        public string BomCode { get; set; } = string.Empty;
        public string FinishedProductName { get; set; } = string.Empty;
        public string RawMaterialName { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class ManufacturingOrderDto : EntityDto<Guid>
    {
        public string MoNumber { get; set; } = string.Empty;
        public string FinishedProductName { get; set; } = string.Empty;
        public int QuantityToProduce { get; set; }
        public string WorkCenter { get; set; } = "Assembly Line A";
        public DateTime ScheduledStartDate { get; set; }
        public string Status { get; set; } = "In Production";
    }

    public class FixedAssetDto : EntityDto<Guid>
    {
        public string AssetCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Machinery";
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal DepreciationRateAnnual { get; set; }
        public string Location { get; set; } = "Cairo Plant";
        public string AssignedEmployee { get; set; } = string.Empty;
    }

    public class MaintenanceRequestDto : EntityDto<Guid>
    {
        public string WorkOrderCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = "Preventive";
        public string TechnicianName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Status { get; set; } = "In Progress";
        public DateTime ScheduledDate { get; set; }
    }

    // AppServices Implementations
    public class CompanyAppService : CrudAppService<Company, CompanyDto, Guid, PagedAndSortedResultRequestDto, CompanyDto> { public CompanyAppService(IRepository<Company, Guid> repository) : base(repository) { } }
    public class BranchAppService : CrudAppService<Branch, BranchDto, Guid, PagedAndSortedResultRequestDto, BranchDto> { public BranchAppService(IRepository<Branch, Guid> repository) : base(repository) { } }
    public class CostCenterAppService : CrudAppService<CostCenter, CostCenterDto, Guid, PagedAndSortedResultRequestDto, CostCenterDto> { public CostCenterAppService(IRepository<CostCenter, Guid> repository) : base(repository) { } }
    public class FiscalYearAppService : CrudAppService<FiscalYear, FiscalYearDto, Guid, PagedAndSortedResultRequestDto, FiscalYearDto>
    {
        public FiscalYearAppService(IRepository<FiscalYear, Guid> repository) : base(repository) { }
        public async Task SetCurrentAsync(Guid id) { var years = await Repository.GetListAsync(); foreach (var yr in years) { yr.IsCurrent = (yr.Id == id); await Repository.UpdateAsync(yr); } }
    }
    public class CurrencyAppService : CrudAppService<Currency, CurrencyDto, Guid, PagedAndSortedResultRequestDto, CurrencyDto> { public CurrencyAppService(IRepository<Currency, Guid> repository) : base(repository) { } }
    public class TaxConfigAppService : CrudAppService<TaxConfig, TaxConfigDto, Guid, PagedAndSortedResultRequestDto, TaxConfigDto> { public TaxConfigAppService(IRepository<TaxConfig, Guid> repository) : base(repository) { } }
    public class PaymentTermAppService : CrudAppService<PaymentTerm, PaymentTermDto, Guid, PagedAndSortedResultRequestDto, PaymentTermDto> { public PaymentTermAppService(IRepository<PaymentTerm, Guid> repository) : base(repository) { } }
    /// <summary>
    /// Leads with the full qualification funnel:
    /// New → Contacted → Qualified → Converted, or → Lost at any point.
    /// GET/POST/PUT/DELETE /api/hr/lead
    /// </summary>
    public class LeadAppService : CrudAppService<Lead, LeadDto, Guid, LeadListInput, LeadDto>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<Deal, Guid> _dealRepository;
        private readonly IRepository<CrmContact, Guid> _contactRepository;

        public LeadAppService(
            IRepository<Lead, Guid> repository,
            IRepository<Customer, Guid> customerRepository,
            IRepository<Deal, Guid> dealRepository,
            IRepository<CrmContact, Guid> contactRepository) : base(repository)
        {
            _customerRepository = customerRepository;
            _dealRepository = dealRepository;
            _contactRepository = contactRepository;
        }

        protected override async Task<IQueryable<Lead>> CreateFilteredQueryAsync(LeadListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                query = query.Where(x => x.Status == input.Status);
            }

            if (!string.IsNullOrWhiteSpace(input.Source))
            {
                query = query.Where(x => x.Source == input.Source);
            }

            if (!string.IsNullOrWhiteSpace(input.Owner))
            {
                query = query.Where(x => x.SalespersonName == input.Owner);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var f = input.Filter.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(f) ||
                    x.CompanyName.ToLower().Contains(f) ||
                    x.Email.ToLower().Contains(f) ||
                    x.Phone.ToLower().Contains(f));
            }

            if (input.OnlyOverdueFollowUp == true)
            {
                var now = Clock.Now;
                query = query.Where(x =>
                    x.NextFollowUp < now &&
                    x.Status != "Converted" &&
                    x.Status != "Lost");
            }

            return query;
        }

        public override async Task<LeadDto> CreateAsync(LeadDto input)
        {
            var lead = ObjectMapper.Map<LeadDto, Lead>(input);
            if (string.IsNullOrWhiteSpace(lead.Status))
            {
                lead.Status = "New";
            }

            lead.Score = ScoreOf(lead);
            await Repository.InsertAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        public override async Task<LeadDto> UpdateAsync(Guid id, LeadDto input)
        {
            var lead = await Repository.GetAsync(id);
            ObjectMapper.Map(input, lead);
            await Repository.UpdateAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        /// <summary>POST /api/hr/lead/{id}/mark-contacted</summary>
        public async Task<LeadDto> MarkContactedAsync(Guid id)
        {
            var lead = await Repository.GetAsync(id);
            lead.Status = "Contacted";
            lead.Score = ScoreOf(lead);
            await Repository.UpdateAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        /// <summary>POST /api/hr/lead/{id}/qualify — New/Contacted → Qualified.</summary>
        public async Task<LeadDto> QualifyAsync(Guid id)
        {
            var lead = await Repository.GetAsync(id);
            lead.Status = "Qualified";
            lead.QualifiedAt = Clock.Now;
            lead.Score = ScoreOf(lead);
            await Repository.UpdateAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        /// <summary>POST /api/hr/lead/{id}/mark-unqualified — lose the lead with a reason.</summary>
        public async Task<LeadDto> MarkUnqualifiedAsync(Guid id, LeadLostInput input)
        {
            var lead = await Repository.GetAsync(id);
            lead.Status = "Lost";
            lead.LostReason = input?.Reason ?? string.Empty;
            await Repository.UpdateAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        /// <summary>POST /api/hr/lead/{id}/assign</summary>
        public async Task<LeadDto> AssignAsync(Guid id, AssignLeadInput input)
        {
            var lead = await Repository.GetAsync(id);
            lead.SalespersonName = input?.SalespersonName ?? lead.SalespersonName;
            lead.OwnerUserId = input?.OwnerUserId;
            await Repository.UpdateAsync(lead, autoSave: true);
            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        /// <summary>
        /// POST /api/hr/lead/{id}/convert-to-opportunity — the Lead → Opportunity → Account
        /// hand-off: creates (or reuses) the Customer, opens an opportunity and copies the
        /// lead across as the primary contact.
        /// </summary>
        public async Task<LeadDto> ConvertToOpportunityAsync(Guid id)
        {
            var lead = await Repository.GetAsync(id);

            if (lead.Status == "Converted" && lead.ConvertedDealId.HasValue)
            {
                return ObjectMapper.Map<Lead, LeadDto>(lead);
            }

            var customer = await FindOrCreateCustomerAsync(lead);

            // Carry the lead over as the primary contact of the new account.
            var existing = await _contactRepository.FindAsync(c => c.LeadId == lead.Id);
            if (existing == null)
            {
                var leadName = string.IsNullOrWhiteSpace(lead.Name) ? string.Empty : lead.Name;
                var parts = leadName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var firstName = parts.Length > 0 ? parts[0] : leadName;
                var lastName = parts.Length > 1 ? parts[1] : string.Empty;

                await _contactRepository.InsertAsync(new CrmContact
                {
                    CustomerId = customer.Id,
                    LeadId = lead.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    FullName = leadName,
                    Email = lead.Email,
                    Phone = lead.Phone,
                    IsPrimary = true,
                    Notes = lead.Notes
                }, autoSave: true);
            }

            var opportunityTitle = string.IsNullOrWhiteSpace(lead.CompanyName)
                ? lead.Name
                : lead.CompanyName;

            var deal = new Deal
            {
                Title = opportunityTitle + " — opportunity",
                CustomerName = customer.Name,
                CustomerId = customer.Id,
                LeadId = lead.Id,
                Value = 0,
                Stage = "New",
                Probability = 10,
                ExpectedCloseDate = Clock.Now.AddDays(30),
                OwnerName = string.IsNullOrWhiteSpace(lead.SalespersonName)
                    ? "Account Executive"
                    : lead.SalespersonName
            };

            await _dealRepository.InsertAsync(deal, autoSave: true);

            lead.Status = "Converted";
            lead.ConvertedAt = Clock.Now;
            lead.ConvertedCustomerId = customer.Id;
            lead.ConvertedDealId = deal.Id;
            lead.Score = 100;

            await Repository.UpdateAsync(lead, autoSave: true);

            return ObjectMapper.Map<Lead, LeadDto>(lead);
        }

        private async Task<Customer> FindOrCreateCustomerAsync(Lead lead)
        {
            var name = string.IsNullOrWhiteSpace(lead.CompanyName) ? lead.Name : lead.CompanyName;

            var existing = await _customerRepository.FindAsync(c => c.Name == name);
            if (existing != null)
            {
                return existing;
            }

            var code = "CUST-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

            var customer = new Customer
            {
                CustomerCode = code,
                Name = name,
                Email = lead.Email,
                Phone = lead.Phone,
                ContactPerson = lead.Name,
                OwnerName = lead.SalespersonName
            };

            await _customerRepository.InsertAsync(customer, autoSave: true);
            return customer;
        }

        /// <summary>
        /// Simple, transparent scoring heuristic. Kept on the server so the score is the
        /// same wherever it is displayed.
        /// </summary>
        private static int ScoreOf(Lead lead)
        {
            var score = 0;

            if (!string.IsNullOrWhiteSpace(lead.Email)) score += 20;
            if (!string.IsNullOrWhiteSpace(lead.Phone)) score += 15;
            if (!string.IsNullOrWhiteSpace(lead.CompanyName)) score += 15;

            score += lead.Source switch
            {
                "Referral" => 25,
                "Campaign" => 15,
                "Website" => 10,
                "Social Media" => 10,
                _ => 5
            };

            if (lead.Status == "Qualified") score += 25;
            else if (lead.Status == "Contacted") score += 10;
            else if (lead.Status == "Converted") score = 100;
            else if (lead.Status == "Lost") score = 0;

            return Math.Clamp(score, 0, 100);
        }
    }
    public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CustomerDto> { public CustomerAppService(IRepository<Customer, Guid> repository) : base(repository) { } }
    public class SupplierAppService : CrudAppService<Supplier, SupplierDto, Guid, PagedAndSortedResultRequestDto, SupplierDto> { public SupplierAppService(IRepository<Supplier, Guid> repository) : base(repository) { } }
    public class SalesOrderAppService : CrudAppService<SalesOrder, SalesOrderDto, Guid, PagedAndSortedResultRequestDto, SalesOrderDto> { public SalesOrderAppService(IRepository<SalesOrder, Guid> repository) : base(repository) { } public async Task ApproveAsync(Guid id) { var order = await Repository.GetAsync(id); order.Status = "Approved"; await Repository.UpdateAsync(order); } }
    public class DeliveryNoteAppService : CrudAppService<DeliveryNote, DeliveryNoteDto, Guid, PagedAndSortedResultRequestDto, DeliveryNoteDto>
    {
        public DeliveryNoteAppService(IRepository<DeliveryNote, Guid> repository) : base(repository) { }
        public async Task CaptureSignatureAsync(Guid id, string signatureBase64, string signedBy)
        {
            var dn = await Repository.GetAsync(id);
            dn.SignatureBase64 = signatureBase64;
            dn.SignedBy = signedBy;
            dn.SignedAt = DateTime.UtcNow;
            dn.Status = "Delivered";
            await Repository.UpdateAsync(dn);
        }
        public async Task UpdateTrackingAsync(Guid id, string trackingNumber, string currentLocation)
        {
            var dn = await Repository.GetAsync(id);
            dn.TrackingNumber = trackingNumber;
            dn.CurrentLocation = currentLocation;
            await Repository.UpdateAsync(dn);
        }
    }
    public class PurchaseRequestAppService : CrudAppService<PurchaseRequest, PurchaseRequestDto, Guid, PagedAndSortedResultRequestDto, PurchaseRequestDto> { public PurchaseRequestAppService(IRepository<PurchaseRequest, Guid> repository) : base(repository) { } public async Task ApproveAsync(Guid id) { var pr = await Repository.GetAsync(id); pr.Status = "Approved"; await Repository.UpdateAsync(pr); } }
    public class RfqAppService : CrudAppService<Rfq, RfqDto, Guid, PagedAndSortedResultRequestDto, RfqDto> { public RfqAppService(IRepository<Rfq, Guid> repository) : base(repository) { } }
    public class GoodsReceiptAppService : CrudAppService<GoodsReceipt, GoodsReceiptDto, Guid, PagedAndSortedResultRequestDto, GoodsReceiptDto>
    {
        public GoodsReceiptAppService(IRepository<GoodsReceipt, Guid> repository) : base(repository) { }
        public async Task PassQualityCheckAsync(Guid id)
        {
            var gr = await Repository.GetAsync(id);
            gr.QcStatus = "Passed";
            await Repository.UpdateAsync(gr);
        }
    }

    // AppServices for Modules 16-20
    public class ExpenseAppService : CrudAppService<ExpenseRequest, ExpenseRequestDto, Guid, PagedAndSortedResultRequestDto, ExpenseRequestDto>
    {
        public ExpenseAppService(IRepository<ExpenseRequest, Guid> repository) : base(repository) { }
        public async Task ApproveAsync(Guid id) { var exp = await Repository.GetAsync(id); exp.Status = "Approved"; await Repository.UpdateAsync(exp); }
    }

    public class ProjectAppService : CrudAppService<Project, ProjectDto, Guid, PagedAndSortedResultRequestDto, ProjectDto>
    {
        public ProjectAppService(IRepository<Project, Guid> repository) : base(repository) { }
    }

    public class ManufacturingAppService : CrudAppService<ManufacturingOrder, ManufacturingOrderDto, Guid, PagedAndSortedResultRequestDto, ManufacturingOrderDto>
    {
        public ManufacturingAppService(IRepository<ManufacturingOrder, Guid> repository) : base(repository) { }
        public async Task CompleteOrderAsync(Guid id) { var mo = await Repository.GetAsync(id); mo.Status = "Completed"; await Repository.UpdateAsync(mo); }
    }

    public class AssetAppService : CrudAppService<FixedAsset, FixedAssetDto, Guid, PagedAndSortedResultRequestDto, FixedAssetDto>
    {
        public AssetAppService(IRepository<FixedAsset, Guid> repository) : base(repository) { }
    }

    public class MaintenanceAppService : CrudAppService<MaintenanceRequest, MaintenanceRequestDto, Guid, PagedAndSortedResultRequestDto, MaintenanceRequestDto>
    {
        public MaintenanceAppService(IRepository<MaintenanceRequest, Guid> repository) : base(repository) { }
    }
}
