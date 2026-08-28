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
    // DTOs
    public class CompanyDto : EntityDto<Guid>
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

    public class BranchDto : EntityDto<Guid>
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsHeadquarters { get; set; }
        public bool IsActive { get; set; }
    }

    public class CostCenterDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class FiscalYearDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsClosed { get; set; }
    }

    public class CurrencyDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = "$";
        public decimal ExchangeRate { get; set; }
        public bool IsBase { get; set; }
        public bool IsActive { get; set; }
    }

    public class TaxConfigDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string TaxType { get; set; } = "VAT";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class PaymentTermDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public int DueDays { get; set; }
        public int DiscountDays { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // Services
    public class CompanyAppService : CrudAppService<Company, CompanyDto, Guid, PagedAndSortedResultRequestDto, CompanyDto>
    {
        public CompanyAppService(IRepository<Company, Guid> repository) : base(repository) { }
    }

    public class BranchAppService : CrudAppService<Branch, BranchDto, Guid, PagedAndSortedResultRequestDto, BranchDto>
    {
        public BranchAppService(IRepository<Branch, Guid> repository) : base(repository) { }
    }

    public class CostCenterAppService : CrudAppService<CostCenter, CostCenterDto, Guid, PagedAndSortedResultRequestDto, CostCenterDto>
    {
        public CostCenterAppService(IRepository<CostCenter, Guid> repository) : base(repository) { }
    }

    public class FiscalYearAppService : CrudAppService<FiscalYear, FiscalYearDto, Guid, PagedAndSortedResultRequestDto, FiscalYearDto>
    {
        public FiscalYearAppService(IRepository<FiscalYear, Guid> repository) : base(repository) { }

        public async Task SetCurrentAsync(Guid id)
        {
            var years = await Repository.GetListAsync();
            foreach (var yr in years)
            {
                yr.IsCurrent = (yr.Id == id);
                await Repository.UpdateAsync(yr);
            }
        }
    }

    public class CurrencyAppService : CrudAppService<Currency, CurrencyDto, Guid, PagedAndSortedResultRequestDto, CurrencyDto>
    {
        public CurrencyAppService(IRepository<Currency, Guid> repository) : base(repository) { }
    }

    public class TaxConfigAppService : CrudAppService<TaxConfig, TaxConfigDto, Guid, PagedAndSortedResultRequestDto, TaxConfigDto>
    {
        public TaxConfigAppService(IRepository<TaxConfig, Guid> repository) : base(repository) { }
    }

    public class PaymentTermAppService : CrudAppService<PaymentTerm, PaymentTermDto, Guid, PagedAndSortedResultRequestDto, PaymentTermDto>
    {
        public PaymentTermAppService(IRepository<PaymentTerm, Guid> repository) : base(repository) { }
    }
}
