using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Domain.Subscriptions
{
    public class SubscriptionDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Plan, Guid> _planRepository;
        private readonly IRepository<Feature, Guid> _featureRepository;
        private readonly IRepository<PlanFeature, Guid> _planFeatureRepository;

        public SubscriptionDataSeedContributor(
            IRepository<Plan, Guid> planRepository,
            IRepository<Feature, Guid> featureRepository,
            IRepository<PlanFeature, Guid> planFeatureRepository)
        {
            _planRepository = planRepository;
            _featureRepository = featureRepository;
            _planFeatureRepository = planFeatureRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _featureRepository.GetCountAsync() > 0) return;

            // 1. Seed Core Features
            var fInvoices = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.InvoicesMonthly, Name = "Monthly Invoices", Category = "Sales", ValueType = "Integer", Unit = "per_month" });
            var fUsers = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.UsersMax, Name = "Maximum Users", Category = "Admin", ValueType = "Integer", Unit = "count" });
            var fBranches = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.BranchesMax, Name = "Maximum Branches", Category = "Org", ValueType = "Integer", Unit = "count" });
            var fEmployees = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.EmployeesMax, Name = "Maximum Employees", Category = "HR", ValueType = "Integer", Unit = "count" });
            var fQuotations = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.QuotationsMonthly, Name = "Monthly Quotations", Category = "Sales", ValueType = "Integer", Unit = "per_month" });
            var fStorage = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.StorageMaxGb, Name = "Storage Allocation (GB)", Category = "System", ValueType = "Integer", Unit = "GB" });
            var fReports = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.AdvancedReports, Name = "Advanced Reports & AI", Category = "Analytics", ValueType = "Boolean", Unit = "boolean" });
            var fApi = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.ApiAccess, Name = "REST API Access", Category = "System", ValueType = "Boolean", Unit = "boolean" });
            var fCurrency = await _featureRepository.InsertAsync(new Feature { Code = ErpFeatures.MultiCurrency, Name = "Multi-Currency Engine", Category = "Finance", ValueType = "Boolean", Unit = "boolean" });

            // 2. Seed Default Plans
            var pFree = await _planRepository.InsertAsync(new Plan { Code = "FREE", Name = "Free Plan", Description = "Starter tier for small business trials", Price = 0, Currency = "USD", BillingPeriod = "Monthly", DisplayOrder = 1 });
            var pBasic = await _planRepository.InsertAsync(new Plan { Code = "BASIC", Name = "Basic Plan", Description = "Ideal for growing small teams", Price = 29, Currency = "USD", BillingPeriod = "Monthly", DisplayOrder = 2 });
            var pPro = await _planRepository.InsertAsync(new Plan { Code = "PROFESSIONAL", Name = "Professional Plan", Description = "Full feature suite for expanding enterprises", Price = 99, Currency = "USD", BillingPeriod = "Monthly", DisplayOrder = 3 });
            var pEnt = await _planRepository.InsertAsync(new Plan { Code = "ENTERPRISE", Name = "Enterprise Plan", Description = "Unlimited corporate capability & priority SLA", Price = 299, Currency = "USD", BillingPeriod = "Monthly", DisplayOrder = 4 });

            // 3. Map Free Plan Features
            await AddFeature(pFree.Id, fInvoices.Id, fInvoices.Code, true, 20, "Monthly");
            await AddFeature(pFree.Id, fUsers.Id, fUsers.Code, true, 2, "Fixed");
            await AddFeature(pFree.Id, fBranches.Id, fBranches.Code, true, 1, "Fixed");
            await AddFeature(pFree.Id, fEmployees.Id, fEmployees.Code, true, 10, "Fixed");
            await AddFeature(pFree.Id, fQuotations.Id, fQuotations.Code, true, 20, "Monthly");
            await AddFeature(pFree.Id, fStorage.Id, fStorage.Code, true, 1, "Fixed");
            await AddFeature(pFree.Id, fReports.Id, fReports.Code, false, null, "Boolean");
            await AddFeature(pFree.Id, fApi.Id, fApi.Code, false, null, "Boolean");
            await AddFeature(pFree.Id, fCurrency.Id, fCurrency.Code, false, null, "Boolean");

            // 4. Map Basic Plan Features
            await AddFeature(pBasic.Id, fInvoices.Id, fInvoices.Code, true, 100, "Monthly");
            await AddFeature(pBasic.Id, fUsers.Id, fUsers.Code, true, 5, "Fixed");
            await AddFeature(pBasic.Id, fBranches.Id, fBranches.Code, true, 2, "Fixed");
            await AddFeature(pBasic.Id, fEmployees.Id, fEmployees.Code, true, 50, "Fixed");
            await AddFeature(pBasic.Id, fQuotations.Id, fQuotations.Code, true, 100, "Monthly");
            await AddFeature(pBasic.Id, fStorage.Id, fStorage.Code, true, 5, "Fixed");
            await AddFeature(pBasic.Id, fReports.Id, fReports.Code, false, null, "Boolean");
            await AddFeature(pBasic.Id, fApi.Id, fApi.Code, false, null, "Boolean");
            await AddFeature(pBasic.Id, fCurrency.Id, fCurrency.Code, true, null, "Boolean");

            // 5. Map Professional Plan Features
            await AddFeature(pPro.Id, fInvoices.Id, fInvoices.Code, true, 1000, "Monthly");
            await AddFeature(pPro.Id, fUsers.Id, fUsers.Code, true, 20, "Fixed");
            await AddFeature(pPro.Id, fBranches.Id, fBranches.Code, true, 10, "Fixed");
            await AddFeature(pPro.Id, fEmployees.Id, fEmployees.Code, true, 500, "Fixed");
            await AddFeature(pPro.Id, fQuotations.Id, fQuotations.Code, true, 1000, "Monthly");
            await AddFeature(pPro.Id, fStorage.Id, fStorage.Code, true, 50, "Fixed");
            await AddFeature(pPro.Id, fReports.Id, fReports.Code, true, null, "Boolean");
            await AddFeature(pPro.Id, fApi.Id, fApi.Code, true, null, "Boolean");
            await AddFeature(pPro.Id, fCurrency.Id, fCurrency.Code, true, null, "Boolean");

            // 6. Map Enterprise Plan Features (Unlimited)
            await AddFeature(pEnt.Id, fInvoices.Id, fInvoices.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fUsers.Id, fUsers.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fBranches.Id, fBranches.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fEmployees.Id, fEmployees.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fQuotations.Id, fQuotations.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fStorage.Id, fStorage.Code, true, null, "Unlimited");
            await AddFeature(pEnt.Id, fReports.Id, fReports.Code, true, null, "Boolean");
            await AddFeature(pEnt.Id, fApi.Id, fApi.Code, true, null, "Boolean");
            await AddFeature(pEnt.Id, fCurrency.Id, fCurrency.Code, true, null, "Boolean");
        }

        private async Task AddFeature(Guid planId, Guid featureId, string featureCode, bool isEnabled, long? limitValue, string limitType)
        {
            await _planFeatureRepository.InsertAsync(new PlanFeature
            {
                PlanId = planId,
                FeatureId = featureId,
                FeatureCode = featureCode,
                IsEnabled = isEnabled,
                LimitValue = limitValue,
                LimitType = limitType
            });
        }
    }
}
