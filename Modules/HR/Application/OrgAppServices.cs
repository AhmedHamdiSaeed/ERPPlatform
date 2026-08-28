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
    public class CompanyDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string TaxNumber { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string Country { get; set; } = "Egypt"; public string Currency { get; set; } = "USD"; public string Website { get; set; } = string.Empty; public string LogoUrl { get; set; } = string.Empty; public bool IsActive { get; set; } = true; }
    public class BranchDto : EntityDto<Guid> { public Guid CompanyId { get; set; } public string CompanyName { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public bool IsHeadquarters { get; set; } public bool IsActive { get; set; } }
    public class CostCenterDto : EntityDto<Guid> { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class FiscalYearDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public bool IsCurrent { get; set; } public bool IsClosed { get; set; } }
    public class CurrencyDto : EntityDto<Guid> { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Symbol { get; set; } = "$"; public decimal ExchangeRate { get; set; } public bool IsBase { get; set; } public bool IsActive { get; set; } }
    public class TaxConfigDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public decimal Rate { get; set; } public string TaxType { get; set; } = "VAT"; public bool IsDefault { get; set; } public bool IsActive { get; set; } }
    public class PaymentTermDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public int DueDays { get; set; } public int DiscountDays { get; set; } public decimal DiscountPercent { get; set; } public string Description { get; set; } = string.Empty; }
    public class LeadDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Source { get; set; } = "Website"; public string Status { get; set; } = "New"; public string SalespersonName { get; set; } = string.Empty; public DateTime NextFollowUp { get; set; } public string Notes { get; set; } = string.Empty; }
    public class CustomerDto : EntityDto<Guid> { public string CustomerCode { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string ContactPerson { get; set; } = string.Empty; public string TaxNumber { get; set; } = string.Empty; public decimal CreditLimit { get; set; } public decimal OutstandingBalance { get; set; } public string PaymentTerms { get; set; } = "Net 30 Days"; public string Currency { get; set; } = "USD"; public bool IsActive { get; set; } = true; }
    public class SupplierDto : EntityDto<Guid> { public string SupplierCode { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string ContactPerson { get; set; } = string.Empty; public string TaxNumber { get; set; } = string.Empty; public decimal CreditLimit { get; set; } public decimal OutstandingBalance { get; set; } public string PaymentTerms { get; set; } = "Net 30 Days"; public bool IsActive { get; set; } = true; }
    public class SalesOrderDto : EntityDto<Guid> { public string OrderNumber { get; set; } = string.Empty; public string CustomerName { get; set; } = string.Empty; public DateTime OrderDate { get; set; } public DateTime ExpectedDeliveryDate { get; set; } public decimal Subtotal { get; set; } public decimal TaxAmount { get; set; } public decimal TotalAmount { get; set; } public string Status { get; set; } = "Draft"; public string Notes { get; set; } = string.Empty; }
    public class DeliveryNoteDto : EntityDto<Guid> { public string DeliveryNumber { get; set; } = string.Empty; public string SalesOrderNumber { get; set; } = string.Empty; public string CustomerName { get; set; } = string.Empty; public DateTime DeliveryDate { get; set; } public string DispatchWarehouse { get; set; } = "Main Warehouse"; public string Status { get; set; } = "Dispatched"; public string Carrier { get; set; } = "Express Freight"; }
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
    public class LeadAppService : CrudAppService<Lead, LeadDto, Guid, PagedAndSortedResultRequestDto, LeadDto> { public LeadAppService(IRepository<Lead, Guid> repository) : base(repository) { } public async Task ConvertToOpportunityAsync(Guid id) { var lead = await Repository.GetAsync(id); lead.Status = "Opportunity"; await Repository.UpdateAsync(lead); } }
    public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CustomerDto> { public CustomerAppService(IRepository<Customer, Guid> repository) : base(repository) { } }
    public class SupplierAppService : CrudAppService<Supplier, SupplierDto, Guid, PagedAndSortedResultRequestDto, SupplierDto> { public SupplierAppService(IRepository<Supplier, Guid> repository) : base(repository) { } }
    public class SalesOrderAppService : CrudAppService<SalesOrder, SalesOrderDto, Guid, PagedAndSortedResultRequestDto, SalesOrderDto> { public SalesOrderAppService(IRepository<SalesOrder, Guid> repository) : base(repository) { } public async Task ApproveAsync(Guid id) { var order = await Repository.GetAsync(id); order.Status = "Approved"; await Repository.UpdateAsync(order); } }
    public class DeliveryNoteAppService : CrudAppService<DeliveryNote, DeliveryNoteDto, Guid, PagedAndSortedResultRequestDto, DeliveryNoteDto> { public DeliveryNoteAppService(IRepository<DeliveryNote, Guid> repository) : base(repository) { } }
    public class PurchaseRequestAppService : CrudAppService<PurchaseRequest, PurchaseRequestDto, Guid, PagedAndSortedResultRequestDto, PurchaseRequestDto> { public PurchaseRequestAppService(IRepository<PurchaseRequest, Guid> repository) : base(repository) { } public async Task ApproveAsync(Guid id) { var pr = await Repository.GetAsync(id); pr.Status = "Approved"; await Repository.UpdateAsync(pr); } }
    public class RfqAppService : CrudAppService<Rfq, RfqDto, Guid, PagedAndSortedResultRequestDto, RfqDto> { public RfqAppService(IRepository<Rfq, Guid> repository) : base(repository) { } }
    public class GoodsReceiptAppService : CrudAppService<GoodsReceipt, GoodsReceiptDto, Guid, PagedAndSortedResultRequestDto, GoodsReceiptDto> { public GoodsReceiptAppService(IRepository<GoodsReceipt, Guid> repository) : base(repository) { } }

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
