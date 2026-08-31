using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class StockCountDto : EntityDto<Guid>
    {
        public string CountNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime CountDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string CountedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class StockCountItemDto : EntityDto<Guid>
    {
        public Guid StockCountId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int SystemStock { get; set; }
        public int CountedStock { get; set; }
        public int Discrepancy { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateStockCountDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class StockCountAppService : CrudAppService<StockCount, StockCountDto, Guid, PagedAndSortedResultRequestDto, CreateStockCountDto>
    {
        private readonly IRepository<StockCountItem, Guid> _itemRepository;

        public StockCountAppService(
            IRepository<StockCount, Guid> repository,
            IRepository<StockCountItem, Guid> itemRepository) : base(repository)
        {
            _itemRepository = itemRepository;
        }

        public async Task SubmitAsync(Guid id)
        {
            var sc = await Repository.GetAsync(id);
            sc.Status = "Submitted";
            await Repository.UpdateAsync(sc);
        }

        public async Task ApproveAsync(Guid id)
        {
            var sc = await Repository.GetAsync(id);
            sc.Status = "Approved";
            sc.ApprovedBy = CurrentUser.UserName ?? "System";
            sc.ApprovedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(sc);
        }

        public async Task<ListResultDto<StockCountItemDto>> GetDiscrepanciesAsync(Guid stockCountId)
        {
            var items = await _itemRepository.GetListAsync();
            var dtos = items
                .Where(i => i.StockCountId == stockCountId && i.Discrepancy != 0)
                .Select(i => new StockCountItemDto
                {
                    Id = i.Id,
                    StockCountId = i.StockCountId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    Barcode = i.Barcode,
                    SystemStock = i.SystemStock,
                    CountedStock = i.CountedStock,
                    Discrepancy = i.Discrepancy,
                    Notes = i.Notes
                }).ToList();
            return new ListResultDto<StockCountItemDto>(dtos);
        }

        public async Task<StockCountItemDto> AddItemAsync(StockCountItemDto input)
        {
            var entity = new StockCountItem
            {
                StockCountId = input.StockCountId,
                ProductId = input.ProductId,
                ProductName = input.ProductName,
                Sku = input.Sku,
                Barcode = input.Barcode,
                SystemStock = input.SystemStock,
                CountedStock = input.CountedStock,
                Discrepancy = input.CountedStock - input.SystemStock,
                Notes = input.Notes
            };
            await _itemRepository.InsertAsync(entity);
            return input;
        }

        protected override Task<StockCount> MapToEntityAsync(CreateStockCountDto createInput)
        {
            return Task.FromResult(new StockCount
            {
                CountNumber = $"SC-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}",
                WarehouseId = createInput.WarehouseId,
                WarehouseName = createInput.WarehouseName,
                CountDate = DateTime.UtcNow,
                Status = "Draft",
                CountedBy = CurrentUser.UserName ?? "System",
                Notes = createInput.Notes
            });
        }

        protected override Task<StockCountDto> MapToGetOutputDtoAsync(StockCount entity)
        {
            return Task.FromResult(new StockCountDto
            {
                Id = entity.Id,
                CountNumber = entity.CountNumber,
                WarehouseId = entity.WarehouseId,
                WarehouseName = entity.WarehouseName,
                CountDate = entity.CountDate,
                Status = entity.Status,
                CountedBy = entity.CountedBy,
                ApprovedBy = entity.ApprovedBy,
                ApprovedAt = entity.ApprovedAt,
                Notes = entity.Notes
            });
        }
    }
}
