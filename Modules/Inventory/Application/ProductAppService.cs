using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using EntityNotFoundException = global::Volo.Abp.Domain.Entities.EntityNotFoundException;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class ProductDto : EntityDto<Guid>
    {
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
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
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public string Unit { get; set; } = "pcs";
        public string WarehouseName { get; set; } = "Main Warehouse";
        public string SupplierName { get; set; } = string.Empty;
    }

    public class ProductGetListInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } = string.Empty; // Search by name, SKU, barcode, supplier
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // In Stock, Low Stock, Out of Stock
        public string WarehouseName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? LowStockOnly { get; set; }
    }

    public interface IProductAppService : ICrudAppService<ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>
    {
        Task AdjustStockAsync(Guid id, int newStock);
        Task<ProductDto> GetByBarcodeAsync(string barcode);
        Task<PagedResultDto<ProductDto>> GetListFilteredAsync(ProductGetListInput input);
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

        public async Task<ProductDto> GetByBarcodeAsync(string barcode)
        {
            var products = await Repository.GetListAsync();
            var product = products.FirstOrDefault(p => p.Barcode == barcode);
            if (product == null)
            {
                throw new EntityNotFoundException($"Product with barcode '{barcode}' not found.");
            }
            return await MapToGetOutputDtoAsync(product);
        }

        public async Task<PagedResultDto<ProductDto>> GetListFilteredAsync(ProductGetListInput input)
        {
            var all = await Repository.GetListAsync();
            var query = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var filter = input.Filter.ToLowerInvariant();
                query = query.Where(p =>
                    (p.Name ?? "").ToLower().Contains(filter) ||
                    (p.Sku ?? "").ToLower().Contains(filter) ||
                    (p.Barcode ?? "").ToLower().Contains(filter) ||
                    (p.SupplierName ?? "").ToLower().Contains(filter));
            }

            if (!string.IsNullOrWhiteSpace(input.Category))
            {
                query = query.Where(p => p.Category == input.Category);
            }

            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                query = query.Where(p => p.Status == input.Status);
            }

            if (!string.IsNullOrWhiteSpace(input.WarehouseName))
            {
                query = query.Where(p => p.WarehouseName == input.WarehouseName);
            }

            if (!string.IsNullOrWhiteSpace(input.SupplierName))
            {
                query = query.Where(p => p.SupplierName == input.SupplierName);
            }

            if (input.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= input.MinPrice.Value);
            }

            if (input.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= input.MaxPrice.Value);
            }

            if (input.LowStockOnly == true)
            {
                query = query.Where(p => p.Stock <= p.ReorderLevel);
            }

            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                query = input.Sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name);
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            var totalCount = query.Count();
            var items = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var dtos = items.Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Barcode = p.Barcode,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock,
                ReorderLevel = p.ReorderLevel,
                Unit = p.Unit,
                WarehouseName = p.WarehouseName,
                Status = p.Status,
                SupplierName = p.SupplierName
            }).ToList();

            return new PagedResultDto<ProductDto>(totalCount, dtos);
        }

        protected override Task<Product> MapToEntityAsync(CreateUpdateProductDto createInput)
        {
            return Task.FromResult(new Product
            {
                Sku = createInput.Sku,
                Barcode = createInput.Barcode,
                Name = createInput.Name,
                Category = createInput.Category,
                Price = createInput.Price,
                Stock = createInput.Stock,
                ReorderLevel = createInput.ReorderLevel,
                Unit = string.IsNullOrWhiteSpace(createInput.Unit) ? "pcs" : createInput.Unit,
                WarehouseName = string.IsNullOrWhiteSpace(createInput.WarehouseName) ? "Main Warehouse" : createInput.WarehouseName,
                SupplierName = createInput.SupplierName,
                Status = createInput.Stock == 0 ? "Out of Stock" : (createInput.Stock <= createInput.ReorderLevel ? "Low Stock" : "In Stock")
            });
        }

        protected override Task MapToEntityAsync(CreateUpdateProductDto updateInput, Product entity)
        {
            entity.Sku = updateInput.Sku;
            entity.Barcode = updateInput.Barcode;
            entity.Name = updateInput.Name;
            entity.Category = updateInput.Category;
            entity.Price = updateInput.Price;
            entity.Stock = updateInput.Stock;
            entity.ReorderLevel = updateInput.ReorderLevel;
            entity.Unit = updateInput.Unit;
            entity.WarehouseName = updateInput.WarehouseName;
            entity.SupplierName = updateInput.SupplierName;
            entity.Status = updateInput.Stock == 0 ? "Out of Stock" : (updateInput.Stock <= updateInput.ReorderLevel ? "Low Stock" : "In Stock");
            return Task.CompletedTask;
        }

        protected override Task<ProductDto> MapToGetOutputDtoAsync(Product entity)
        {
            return Task.FromResult(new ProductDto
            {
                Id = entity.Id,
                Sku = entity.Sku,
                Barcode = entity.Barcode,
                Name = entity.Name,
                Category = entity.Category,
                Price = entity.Price,
                Stock = entity.Stock,
                ReorderLevel = entity.ReorderLevel,
                Unit = entity.Unit,
                WarehouseName = entity.WarehouseName,
                Status = entity.Status,
                SupplierName = entity.SupplierName
            });
        }
    }
}
