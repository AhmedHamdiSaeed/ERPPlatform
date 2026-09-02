using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Domain.Reports
{
    /// <summary>
    /// Seeds the default report catalog so the Report Center is usable on a fresh database
    /// instead of rendering an empty state. Safe to re-run: skips when any report exists.
    /// </summary>
    public class ReportDefinitionDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<ReportDefinition, System.Guid> _reportRepository;

        public ReportDefinitionDataSeedContributor(IRepository<ReportDefinition, System.Guid> reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _reportRepository.GetCountAsync() > 0) return;

            await _reportRepository.InsertAsync(new ReportDefinition
            {
                Title = "Monthly Workforce Analytics & Headcount",
                Category = "HR",
                Description = "Headcount, leave and salary distribution grouped by department.",
                DataSourceCode = ReportDataSources.HrHeadcount
            });

            await _reportRepository.InsertAsync(new ReportDefinition
            {
                Title = "Inventory Valuation & Stock Turnover",
                Category = "Inventory",
                Description = "Stock units and valuation grouped by warehouse.",
                DataSourceCode = ReportDataSources.InventoryValuation
            });

            await _reportRepository.InsertAsync(new ReportDefinition
            {
                Title = "Open Purchase Orders & Supplier Commitments",
                Category = "Financial",
                Description = "Every purchase order not yet received, ordered by delivery date.",
                DataSourceCode = ReportDataSources.OpenPurchaseOrders
            });

            await _reportRepository.InsertAsync(new ReportDefinition
            {
                Title = "Pending Approvals by Type",
                Category = "Workflow",
                Description = "Outstanding leave requests, workflow tasks and purchase orders awaiting a decision.",
                DataSourceCode = ReportDataSources.PendingApprovals
            });
        }
    }
}
