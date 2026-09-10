using AutoMapper;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Modules.HR.Application
{
    public class HRApplicationAutoMapperProfile : Profile
    {
        public HRApplicationAutoMapperProfile()
        {
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Branch, BranchDto>().ReverseMap();
            CreateMap<CostCenter, CostCenterDto>().ReverseMap();
            CreateMap<FiscalYear, FiscalYearDto>().ReverseMap();
            CreateMap<Currency, CurrencyDto>().ReverseMap();
            CreateMap<TaxConfig, TaxConfigDto>().ReverseMap();
            CreateMap<PaymentTerm, PaymentTermDto>().ReverseMap();
            // CreationTime is read-only from the API's point of view: letting it flow back
            // into the entity would stamp 0001-01-01 on every update.
            CreateMap<Lead, LeadDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore());
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<SalesOrder, SalesOrderDto>().ReverseMap();
            CreateMap<DeliveryNote, DeliveryNoteDto>().ReverseMap();
            CreateMap<PurchaseRequest, PurchaseRequestDto>().ReverseMap();
            CreateMap<Rfq, RfqDto>().ReverseMap();
            CreateMap<GoodsReceipt, GoodsReceiptDto>().ReverseMap();
            CreateMap<ExpenseRequest, ExpenseRequestDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<BillOfMaterials, BillOfMaterialsDto>().ReverseMap();
            CreateMap<ManufacturingOrder, ManufacturingOrderDto>().ReverseMap();
            CreateMap<FixedAsset, FixedAssetDto>().ReverseMap();
            CreateMap<MaintenanceRequest, MaintenanceRequestDto>().ReverseMap();
            CreateMap<LeaveRequest, LeaveRequestDto>().ReverseMap();
            CreateMap<Attendance, AttendanceDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<CreateUpdateEmployeeDto, Employee>().ReverseMap();
        }
    }
}