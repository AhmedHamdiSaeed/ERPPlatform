using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Search
{
    public class SearchResultItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
    }

    public class SearchResultGroupDto
    {
        public string Category { get; set; } = string.Empty;
        public List<SearchResultItemDto> Items { get; set; } = new();
    }

    public interface ISearchAppService : IApplicationService
    {
        Task<ListResultDto<SearchResultGroupDto>> GetAllAsync(string query, int maxPerCategory = 5);
    }

    /// <summary>
    /// Global (cross-module) search used by the Ctrl+K palette.
    /// Every query runs server-side with Take() so a large tenant never
    /// pulls whole tables into memory just to answer one keystroke.
    /// </summary>
    public class SearchAppService : ApplicationService, ISearchAppService
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<Department, Guid> _departmentRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepository;
        private readonly IRepository<WorkflowDefinition, Guid> _workflowRepository;
        private readonly IRepository<ReportDefinition, Guid> _reportRepository;

        public SearchAppService(
            IRepository<Employee, Guid> employeeRepository,
            IRepository<Department, Guid> departmentRepository,
            IRepository<Product, Guid> productRepository,
            IRepository<PurchaseOrder, Guid> purchaseOrderRepository,
            IRepository<WorkflowDefinition, Guid> workflowRepository,
            IRepository<ReportDefinition, Guid> reportRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _productRepository = productRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _workflowRepository = workflowRepository;
            _reportRepository = reportRepository;
        }

        public async Task<ListResultDto<SearchResultGroupDto>> GetAllAsync(string query, int maxPerCategory = 5)
        {
            var q = (query ?? string.Empty).Trim();
            if (q.Length < 2)
            {
                return new ListResultDto<SearchResultGroupDto>(new List<SearchResultGroupDto>());
            }

            if (maxPerCategory <= 0) maxPerCategory = 5;
            if (maxPerCategory > 25) maxPerCategory = 25;

            var groups = new List<SearchResultGroupDto>();

            var employeeQuery = await _employeeRepository.GetQueryableAsync();
            var employees = await AsyncExecuter.ToListAsync(
                employeeQuery
                    .Where(e => e.Name.Contains(q) || e.Position.Contains(q) ||
                                e.DepartmentName.Contains(q) || e.Email.Contains(q))
                    .OrderBy(e => e.Name)
                    .Take(maxPerCategory)
                    .Select(e => new { e.Id, e.Name, e.Position, e.DepartmentName, e.Status })
            );

            if (employees.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Employees",
                    Items = employees.Select(e => new SearchResultItemDto
                    {
                        Title = e.Name,
                        Subtitle = $"{e.Position} • {e.DepartmentName}",
                        Link = $"/hr/employees/{e.Id}",
                        Icon = "pi-user",
                        Badge = e.Status
                    }).ToList()
                });
            }

            var productQuery = await _productRepository.GetQueryableAsync();
            var products = await AsyncExecuter.ToListAsync(
                productQuery
                    .Where(p => p.Name.Contains(q) || p.Sku.Contains(q) ||
                                p.Category.Contains(q) || p.SupplierName.Contains(q))
                    .OrderBy(p => p.Name)
                    .Take(maxPerCategory)
                    .Select(p => new { p.Id, p.Name, p.Sku, p.Stock, p.Unit, p.Price, p.Status })
            );

            if (products.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Products & Inventory",
                    Items = products.Select(p => new SearchResultItemDto
                    {
                        Title = p.Name,
                        Subtitle = $"SKU: {p.Sku} • Stock: {p.Stock} {p.Unit} ({p.Price:0.##})",
                        Link = "/inventory/products",
                        Icon = "pi-box",
                        Badge = p.Status
                    }).ToList()
                });
            }

            var workflowQuery = await _workflowRepository.GetQueryableAsync();
            var workflows = await AsyncExecuter.ToListAsync(
                workflowQuery
                    .Where(w => w.Name.Contains(q) || w.Description.Contains(q) || w.Category.Contains(q))
                    .OrderBy(w => w.Name)
                    .Take(maxPerCategory)
                    .Select(w => new { w.Id, w.Name, w.Description, w.Version })
            );

            if (workflows.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Workflows",
                    Items = workflows.Select(w => new SearchResultItemDto
                    {
                        Title = w.Name,
                        Subtitle = w.Description,
                        Link = "/workflow/designer",
                        Icon = "pi-sitemap",
                        Badge = $"v{w.Version}"
                    }).ToList()
                });
            }

            var departmentQuery = await _departmentRepository.GetQueryableAsync();
            var departments = await AsyncExecuter.ToListAsync(
                departmentQuery
                    .Where(d => d.Name.Contains(q) || d.Code.Contains(q))
                    .OrderBy(d => d.Name)
                    .Take(maxPerCategory)
                    .Select(d => new { d.Id, d.Name, d.Code, d.ManagerName })
            );

            if (departments.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Departments",
                    Items = departments.Select(d => new SearchResultItemDto
                    {
                        Title = d.Name,
                        Subtitle = $"Code: {d.Code} • Manager: {d.ManagerName}",
                        Link = "/hr/departments",
                        Icon = "pi-building"
                    }).ToList()
                });
            }

            var purchaseOrderQuery = await _purchaseOrderRepository.GetQueryableAsync();
            var purchaseOrders = await AsyncExecuter.ToListAsync(
                purchaseOrderQuery
                    .Where(o => o.PoNumber.Contains(q) || o.SupplierName.Contains(q))
                    .OrderByDescending(o => o.OrderDate)
                    .Take(maxPerCategory)
                    .Select(o => new { o.Id, o.PoNumber, o.SupplierName, o.GrandTotal, o.OrderDate, o.Status })
            );

            if (purchaseOrders.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Purchase Orders",
                    Items = purchaseOrders.Select(o => new SearchResultItemDto
                    {
                        Title = $"{o.PoNumber} - {o.SupplierName}",
                        Subtitle = $"Grand Total: {o.GrandTotal:N0} • Date: {o.OrderDate:yyyy-MM-dd}",
                        Link = "/inventory/purchase-orders",
                        Icon = "pi-file",
                        Badge = o.Status
                    }).ToList()
                });
            }

            var reportQuery = await _reportRepository.GetQueryableAsync();
            var reports = await AsyncExecuter.ToListAsync(
                reportQuery
                    .Where(r => r.IsEnabled && (r.Title.Contains(q) || r.Description.Contains(q)))
                    .OrderBy(r => r.Title)
                    .Take(maxPerCategory)
                    .Select(r => new { r.Id, r.Title, r.Description, r.Category })
            );

            if (reports.Any())
            {
                groups.Add(new SearchResultGroupDto
                {
                    Category = "Reports",
                    Items = reports.Select(r => new SearchResultItemDto
                    {
                        Title = r.Title,
                        Subtitle = r.Description,
                        Link = "/reports",
                        Icon = "pi-chart-line",
                        Badge = r.Category
                    }).ToList()
                });
            }

            return new ListResultDto<SearchResultGroupDto>(groups);
        }
    }
}
