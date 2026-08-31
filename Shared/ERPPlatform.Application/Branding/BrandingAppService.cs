using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Branding
{
    public class TenantBrandingDto
    {
        public string AppName { get; set; } = "ERPPlatform";
        public string LogoUrl { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#1890ff";
        public string SecondaryColor { get; set; } = "#52c41a";
        public string Theme { get; set; } = "default";
    }

    public interface IBrandingAppService : IApplicationService
    {
        Task<TenantBrandingDto> GetBrandingAsync();
    }

    public class BrandingAppService : ApplicationService, IBrandingAppService
    {
        private readonly IRepository<Company, Guid> _companyRepository;

        public BrandingAppService(IRepository<Company, Guid> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<TenantBrandingDto> GetBrandingAsync()
        {
            var companies = await _companyRepository.GetListAsync();
            var company = companies.FirstOrDefault(c => c.IsActive) ?? companies.FirstOrDefault();

            if (company == null)
            {
                return new TenantBrandingDto();
            }

            return new TenantBrandingDto
            {
                AppName = company.Name,
                LogoUrl = company.LogoUrl,
                PrimaryColor = string.IsNullOrWhiteSpace(company.PrimaryColor) ? "#1890ff" : company.PrimaryColor,
                SecondaryColor = string.IsNullOrWhiteSpace(company.SecondaryColor) ? "#52c41a" : company.SecondaryColor,
                Theme = string.IsNullOrWhiteSpace(company.Theme) ? "default" : company.Theme
            };
        }
    }
}
