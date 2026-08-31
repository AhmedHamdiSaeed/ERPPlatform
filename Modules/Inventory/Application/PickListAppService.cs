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
    public class PickListDto : EntityDto<Guid>
    {
        public string PickNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class PickListItemDto : EntityDto<Guid>
    {
        public Guid PickListId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int PickedQuantity { get; set; }
        public string BinLocation { get; set; } = string.Empty;
        public bool IsPicked { get; set; }
    }

    public class CreatePickListDto
    {
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PickListAppService : CrudAppService<PickList, PickListDto, Guid, PagedAndSortedResultRequestDto, CreatePickListDto>
    {
        private readonly IRepository<PickListItem, Guid> _itemRepository;

        public PickListAppService(
            IRepository<PickList, Guid> repository,
            IRepository<PickListItem, Guid> itemRepository) : base(repository)
        {
            _itemRepository = itemRepository;
        }

        public async Task AssignAsync(Guid id, string assignedTo)
        {
            var pl = await Repository.GetAsync(id);
            pl.AssignedTo = assignedTo;
            pl.AssignedAt = DateTime.UtcNow;
            pl.Status = "Assigned";
            await Repository.UpdateAsync(pl);
        }

        public async Task CompletePickAsync(Guid id)
        {
            var pl = await Repository.GetAsync(id);
            pl.Status = "Completed";
            pl.CompletedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(pl);
        }

        public async Task<ListResultDto<PickListDto>> GetAssignedAsync(string assignedTo)
        {
            var all = await Repository.GetListAsync();
            var filtered = all.Where(p => p.AssignedTo == assignedTo && p.Status != "Completed" && p.Status != "Cancelled")
                .Select(p => new PickListDto
                {
                    Id = p.Id,
                    PickNumber = p.PickNumber,
                    SalesOrderId = p.SalesOrderId,
                    SalesOrderNumber = p.SalesOrderNumber,
                    WarehouseId = p.WarehouseId,
                    WarehouseName = p.WarehouseName,
                    AssignedTo = p.AssignedTo,
                    AssignedAt = p.AssignedAt,
                    Status = p.Status,
                    CompletedAt = p.CompletedAt,
                    Notes = p.Notes
                }).ToList();
            return new ListResultDto<PickListDto>(filtered);
        }

        public async Task<ListResultDto<PickListItemDto>> GetItemsAsync(Guid pickListId)
        {
            var items = await _itemRepository.GetListAsync();
            var dtos = items.Where(i => i.PickListId == pickListId)
                .Select(i => new PickListItemDto
                {
                    Id = i.Id,
                    PickListId = i.PickListId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    Barcode = i.Barcode,
                    RequiredQuantity = i.RequiredQuantity,
                    PickedQuantity = i.PickedQuantity,
                    BinLocation = i.BinLocation,
                    IsPicked = i.IsPicked
                }).ToList();
            return new ListResultDto<PickListItemDto>(dtos);
        }

        public async Task<PickListItemDto> AddItemAsync(PickListItemDto input)
        {
            var entity = new PickListItem
            {
                PickListId = input.PickListId,
                ProductId = input.ProductId,
                ProductName = input.ProductName,
                Sku = input.Sku,
                Barcode = input.Barcode,
                RequiredQuantity = input.RequiredQuantity,
                PickedQuantity = input.PickedQuantity,
                BinLocation = input.BinLocation,
                IsPicked = input.IsPicked
            };
            await _itemRepository.InsertAsync(entity);
            return input;
        }

        protected override Task<PickList> MapToEntityAsync(CreatePickListDto createInput)
        {
            return Task.FromResult(new PickList
            {
                PickNumber = $"PL-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}",
                SalesOrderId = createInput.SalesOrderId,
                SalesOrderNumber = createInput.SalesOrderNumber,
                WarehouseId = createInput.WarehouseId,
                WarehouseName = createInput.WarehouseName,
                Status = "Open",
                Notes = createInput.Notes
            });
        }

        protected override Task<PickListDto> MapToGetOutputDtoAsync(PickList entity)
        {
            return Task.FromResult(new PickListDto
            {
                Id = entity.Id,
                PickNumber = entity.PickNumber,
                SalesOrderId = entity.SalesOrderId,
                SalesOrderNumber = entity.SalesOrderNumber,
                WarehouseId = entity.WarehouseId,
                WarehouseName = entity.WarehouseName,
                AssignedTo = entity.AssignedTo,
                AssignedAt = entity.AssignedAt,
                Status = entity.Status,
                CompletedAt = entity.CompletedAt,
                Notes = entity.Notes
            });
        }
    }
}
