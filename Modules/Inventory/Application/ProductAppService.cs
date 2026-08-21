using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class ProductDto : EntityDto<Guid>
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; } = "pcs";
        public string WarehouseName { get; set; } = "Main Warehouse";
        public string Status { get; set; } = "In Stock";
        public string SupplierName { get; set; } = string.Empty;
    }

    public class CreateUpdateProductDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string Unit { get; set; } = "pcs";
        public string WarehouseName { get; set; } = "Main Warehouse";
        public string SupplierName { get; set; } = string.Empty;
    }

    public interface IProductAppService : ICrudAppService<ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>
    {
        Task AdjustStockAsync(Guid id, int newStock);
    }

    public class ProductAppService : CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>, IProductAppService
    {
        public ProductAppService(IRepository<Product, Guid> repository) : base(repository)
        {
        }

        public async Task AdjustStockAsync(Guid id, int newStock)
        {
            var product = await Repository.GetAsync(id);
            product.Stock = newStock;
            product.Status = newStock == 0 ? "Out of Stock" : (newStock <= product.ReorderLevel ? "Low Stock" : "In Stock");
            await Repository.UpdateAsync(product);
        }
    }
}
