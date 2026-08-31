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
    public class PackListDto : EntityDto<Guid>
    {
        public string PackNumber { get; set; } = string.Empty;
        public Guid? PickListId { get; set; }
        public string PickListNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PackedBy { get; set; } = string.Empty;
        public DateTime PackedAt { get; set; }
        public string Status { get; set; } = "Open";
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string ShippingLabelUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PackListItemDto : EntityDto<Guid>
    {
        public Guid PackListId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int PackedQuantity { get; set; }
        public int ShippedQuantity { get; set; }
        public string PackageType { get; set; } = "Carton";
        public decimal WeightKg { get; set; }
    }

    public class CreatePackListDto
    {
        public Guid? PickListId { get; set; }
        public string PickListNumber { get; set; } = string.Empty;
        public Guid? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PackListAppService : CrudAppService<PackList, PackListDto, Guid, PagedAndSortedResultRequestDto, CreatePackListDto>
    {
        private readonly IRepository<PackListItem, Guid> _itemRepository;

        public PackListAppService(
            IRepository<PackList, Guid> repository,
            IRepository<PackListItem, Guid> itemRepository) : base(repository)
        {
            _itemRepository = itemRepository;
        }

        public async Task CompletePackAsync(Guid id)
        {
            var pl = await Repository.GetAsync(id);
            pl.Status = "Packed";
            pl.PackedBy = CurrentUser.UserName ?? "System";
            pl.PackedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(pl);
        }

        public async Task GenerateShippingLabelAsync(Guid id, string trackingNumber, string carrier)
        {
            var pl = await Repository.GetAsync(id);
            pl.TrackingNumber = trackingNumber;
            pl.Carrier = carrier;
            pl.Status = "Shipped";
            pl.ShippingLabelUrl = $"/labels/{trackingNumber}.pdf";
            await Repository.UpdateAsync(pl);
        }

        public async Task<ListResultDto<PackListItemDto>> GetItemsAsync(Guid packListId)
        {
            var items = await _itemRepository.GetListAsync();
            var dtos = items.Where(i => i.PackListId == packListId)
                .Select(i => new PackListItemDto
                {
                    Id = i.Id,
                    PackListId = i.PackListId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    Barcode = i.Barcode,
                    PackedQuantity = i.PackedQuantity,
                    ShippedQuantity = i.ShippedQuantity,
                    PackageType = i.PackageType,
                    WeightKg = i.WeightKg
                }).ToList();
            return new ListResultDto<PackListItemDto>(dtos);
        }

        public async Task<PackListItemDto> AddItemAsync(PackListItemDto input)
        {
            var entity = new PackListItem
            {
                PackListId = input.PackListId,
                ProductId = input.ProductId,
                ProductName = input.ProductName,
                Sku = input.Sku,
                Barcode = input.Barcode,
                PackedQuantity = input.PackedQuantity,
                ShippedQuantity = input.ShippedQuantity,
                PackageType = input.PackageType,
                WeightKg = input.WeightKg
            };
            await _itemRepository.InsertAsync(entity);
            return input;
        }

        protected override Task<PackList> MapToEntityAsync(CreatePackListDto createInput)
        {
            return Task.FromResult(new PackList
            {
                PackNumber = $"PK-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}",
                PickListId = createInput.PickListId,
                PickListNumber = createInput.PickListNumber,
                SalesOrderId = createInput.SalesOrderId,
                SalesOrderNumber = createInput.SalesOrderNumber,
                CustomerName = createInput.CustomerName,
                Status = "Open",
                Notes = createInput.Notes
            });
        }

        protected override Task<PackListDto> MapToGetOutputDtoAsync(PackList entity)
        {
            return Task.FromResult(new PackListDto
            {
                Id = entity.Id,
                PackNumber = entity.PackNumber,
                PickListId = entity.PickListId,
                PickListNumber = entity.PickListNumber,
                SalesOrderId = entity.SalesOrderId,
                SalesOrderNumber = entity.SalesOrderNumber,
                CustomerName = entity.CustomerName,
                PackedBy = entity.PackedBy,
                PackedAt = entity.PackedAt,
                Status = entity.Status,
                TrackingNumber = entity.TrackingNumber,
                Carrier = entity.Carrier,
                ShippingLabelUrl = entity.ShippingLabelUrl,
                Notes = entity.Notes
            });
        }
    }
}
