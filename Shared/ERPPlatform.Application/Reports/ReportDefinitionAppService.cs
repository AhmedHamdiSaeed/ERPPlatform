using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Reports;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Reports
{
    public class ReportDefinitionDto : EntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Description { get; set; } = string.Empty;
        public DateTime? LastGenerated { get; set; }
        public int RecordCount { get; set; }
        public string DataSourceCode { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }

    public class CreateUpdateReportDefinitionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Description { get; set; } = string.Empty;
        public string DataSourceCode { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }

    public class ReportRowDto
    {
        public List<string> Cells { get; set; } = new();
    }

    public class ReportRunResultDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
        public List<ReportRowDto> Rows { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public int RecordCount { get; set; }
    }

    public interface IReportDefinitionAppService : IApplicationService
    {
        Task<ListResultDto<ReportDefinitionDto>> GetListAsync(string? category = null);
        Task<ReportDefinitionDto> GetAsync(Guid id);
        Task<ReportDefinitionDto> CreateAsync(CreateUpdateReportDefinitionDto input);
        Task<ReportDefinitionDto> UpdateAsync(Guid id, CreateUpdateReportDefinitionDto input);
        Task DeleteAsync(Guid id);
        Task<ReportRunResultDto> RunAsync(Guid id);
    }

    /// <summary>
    /// Report catalog + runner. Each catalog entry points at a DataSourceCode that maps
    /// to a real aggregate query below, so "Generate" and "Export CSV" return live
    /// numbers instead of a canned preview.
    /// </summary>
    public class ReportDefinitionAppService : ApplicationService, IReportDefinitionAppService
    {
        private readonly IRepository<ReportDefinition, Guid> _reportRepository;
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepository;
        private readonly IRepository<LeaveRequest, Guid> _leaveRequestRepository;
        private readonly IRepository<WorkflowTask, Guid> _workflowTaskRepository;

        public ReportDefinitionAppService(
            IRepository<ReportDefinition, Guid> reportRepository,
            IRepository<Employee, Guid> employeeRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<PurchaseOrder, Guid> purchaseOrderRepository,
            IRepository<LeaveRequest, Guid> leaveRequestRepository,
            IRepository<WorkflowTask, Guid> workflowTaskRepository)
        {
            _reportRepository = reportRepository;
            _employeeRepository = employeeRepository;
            _productRepository = productRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _workflowTaskRepository = workflowTaskRepository;
        }

        public async Task<ListResultDto<ReportDefinitionDto>> GetListAsync(string? category = null)
        {
            var query = await _reportRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(r => r.Category == category);
            }

            query = query.Where(r => r.IsEnabled).OrderBy(r => r.Category).ThenBy(r => r.Title);

            var reports = await AsyncExecuter.ToListAsync(query);
            return new ListResultDto<ReportDefinitionDto>(reports.Select(MapToDto).ToList());
        }

        public async Task<ReportDefinitionDto> GetAsync(Guid id)
        {
            return MapToDto(await _reportRepository.GetAsync(id));
        }

        public async Task<ReportDefinitionDto> CreateAsync(CreateUpdateReportDefinitionDto input)
        {
            var report = new ReportDefinition
            {
                Title = input.Title,
                Category = input.Category,
                Description = input.Description,
                DataSourceCode = input.DataSourceCode,
                IsEnabled = input.IsEnabled
            };

            await _reportRepository.InsertAsync(report);
            return MapToDto(report);
        }

        public async Task<ReportDefinitionDto> UpdateAsync(Guid id, CreateUpdateReportDefinitionDto input)
        {
            var report = await _reportRepository.GetAsync(id);

            report.Title = input.Title;
            report.Category = input.Category;
            report.Description = input.Description;
            report.DataSourceCode = input.DataSourceCode;
            report.IsEnabled = input.IsEnabled;

            await _reportRepository.UpdateAsync(report);
            return MapToDto(report);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _reportRepository.DeleteAsync(id);
        }

        public async Task<ReportRunResultDto> RunAsync(Guid id)
        {
            var report = await _reportRepository.GetAsync(id);

            var result = report.DataSourceCode switch
            {
                ReportDataSources.HrHeadcount => await RunHeadcountAsync(),
                ReportDataSources.InventoryValuation => await RunInventoryValuationAsync(),
                ReportDataSources.OpenPurchaseOrders => await RunOpenPurchaseOrdersAsync(),
                ReportDataSources.PendingApprovals => await RunPendingApprovalsAsync(),
                _ => throw new UserFriendlyException(
                    $"Report '{report.Title}' has no runner for data source '{report.DataSourceCode}'.")
            };

            result.Title = report.Title;
            result.Category = report.Category;
            result.GeneratedAt = DateTime.UtcNow;
            result.RecordCount = result.Rows.Count;

            report.LastGenerated = result.GeneratedAt;
            report.RecordCount = result.RecordCount;
            await _reportRepository.UpdateAsync(report);

            return result;
        }

        private async Task<ReportRunResultDto> RunHeadcountAsync()
        {
            var employees = await AsyncExecuter.ToListAsync(await _employeeRepository.GetQueryableAsync());

            var rows = employees
                .Where(e => e.Status != "Inactive")
                .GroupBy(e => string.IsNullOrWhiteSpace(e.DepartmentName) ? "(Unassigned)" : e.DepartmentName)
                .OrderByDescending(g => g.Count())
                .Select(g => new ReportRowDto
                {
                    Cells = new List<string>
                    {
                        g.Key,
                        g.Count().ToString(),
                        g.Count(e => e.Status == "On Leave").ToString(),
                        g.Sum(e => e.Salary).ToString("0.##")
                    }
                })
                .ToList();

            return new ReportRunResultDto
            {
                Columns = new List<string> { "Department", "Headcount", "On Leave", "Total Salary" },
                Rows = rows
            };
        }

        private async Task<ReportRunResultDto> RunInventoryValuationAsync()
        {
            var products = await AsyncExecuter.ToListAsync(await _productRepository.GetQueryableAsync());

            var rows = products
                .GroupBy(p => string.IsNullOrWhiteSpace(p.WarehouseName) ? "(Unassigned)" : p.WarehouseName)
                .OrderByDescending(g => g.Sum(p => p.Stock * p.Price))
                .Select(g => new ReportRowDto
                {
                    Cells = new List<string>
                    {
                        g.Key,
                        g.Count().ToString(),
                        g.Sum(p => p.Stock).ToString("0.##"),
                        g.Sum(p => p.Stock * p.Price).ToString("0.##")
                    }
                })
                .ToList();

            return new ReportRunResultDto
            {
                Columns = new List<string> { "Warehouse", "Products", "Total Units", "Stock Value" },
                Rows = rows
            };
        }

        private async Task<ReportRunResultDto> RunOpenPurchaseOrdersAsync()
        {
            var orders = await AsyncExecuter.ToListAsync(await _purchaseOrderRepository.GetQueryableAsync());

            var openStates = new[] { "Pending Approval", "Approved", "Ordered", "Draft" };

            var rows = orders
                .Where(o => openStates.Contains(o.Status))
                .OrderBy(o => o.DeliveryDate)
                .Select(o => new ReportRowDto
                {
                    Cells = new List<string>
                    {
                        o.PoNumber,
                        o.SupplierName,
                        o.OrderDate.ToString("yyyy-MM-dd"),
                        o.DeliveryDate.ToString("yyyy-MM-dd"),
                        o.GrandTotal.ToString("0.##"),
                        o.Status
                    }
                })
                .ToList();

            return new ReportRunResultDto
            {
                Columns = new List<string>
                    { "PO Number", "Supplier", "Order Date", "Delivery Date", "Grand Total", "Status" },
                Rows = rows
            };
        }

        private async Task<ReportRunResultDto> RunPendingApprovalsAsync()
        {
            var leaveQuery = await _leaveRequestRepository.GetQueryableAsync();
            var pendingLeave = await AsyncExecuter.CountAsync(leaveQuery.Where(l => l.Status == "Pending"));

            var taskQuery = await _workflowTaskRepository.GetQueryableAsync();
            var pendingTasks = await AsyncExecuter.CountAsync(taskQuery.Where(t => t.Status == "Pending"));

            var poQuery = await _purchaseOrderRepository.GetQueryableAsync();
            var pendingOrders = await AsyncExecuter.CountAsync(poQuery.Where(o => o.Status == "Pending Approval"));

            var rows = new List<ReportRowDto>
            {
                new ReportRowDto { Cells = new List<string> { "Leave Requests", pendingLeave.ToString(), "/hr/leave-requests" } },
                new ReportRowDto { Cells = new List<string> { "Workflow Tasks", pendingTasks.ToString(), "/workflow/tasks" } },
                new ReportRowDto { Cells = new List<string> { "Purchase Orders", pendingOrders.ToString(), "/inventory/purchase-orders" } },
                new ReportRowDto { Cells = new List<string> { "Total", (pendingLeave + pendingTasks + pendingOrders).ToString(), "/workflow/tasks" } }
            };

            return new ReportRunResultDto
            {
                Columns = new List<string> { "Approval Type", "Pending", "Open In" },
                Rows = rows
            };
        }

        private static ReportDefinitionDto MapToDto(ReportDefinition report)
        {
            return new ReportDefinitionDto
            {
                Id = report.Id,
                Title = report.Title,
                Category = report.Category,
                Description = report.Description,
                LastGenerated = report.LastGenerated,
                RecordCount = report.RecordCount,
                DataSourceCode = report.DataSourceCode,
                IsEnabled = report.IsEnabled
            };
        }
    }
}
