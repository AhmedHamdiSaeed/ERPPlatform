using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class WarehouseDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public int TotalProductsCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int CapacityPercentage { get; set; }
    }

    public interface IWarehouseAppService : ICrudAppService<WarehouseDto, Guid, PagedAndSortedResultRequestDto, WarehouseDto>
    {
    }

    public class WarehouseAppService : CrudAppService<Warehouse, WarehouseDto, Guid, PagedAndSortedResultRequestDto, WarehouseDto>, IWarehouseAppService
    {
        public WarehouseAppService(IRepository<Warehouse, Guid> repository) : base(repository)
        {
        }

        protected override Task<Warehouse> MapToEntityAsync(WarehouseDto createInput)
        {
            return Task.FromResult(new Warehouse
            {
                Code = createInput.Code,
                Name = createInput.Name,
                Location = createInput.Location,
                Manager = createInput.Manager,
                TotalProductsCount = createInput.TotalProductsCount,
                TotalStockValue = createInput.TotalStockValue,
                CapacityPercentage = Math.Clamp(createInput.CapacityPercentage, 0, 100)
            });
        }

        protected override Task MapToEntityAsync(WarehouseDto updateInput, Warehouse entity)
        {
            entity.Code = updateInput.Code;
            entity.Name = updateInput.Name;
            entity.Location = updateInput.Location;
            entity.Manager = updateInput.Manager;
            entity.TotalProductsCount = updateInput.TotalProductsCount;
            entity.TotalStockValue = updateInput.TotalStockValue;
            entity.CapacityPercentage = Math.Clamp(updateInput.CapacityPercentage, 0, 100);
            return Task.CompletedTask;
        }

        protected override Task<WarehouseDto> MapToGetOutputDtoAsync(Warehouse entity)
        {
            return Task.FromResult(new WarehouseDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Location = entity.Location,
                Manager = entity.Manager,
                TotalProductsCount = entity.TotalProductsCount,
                TotalStockValue = entity.TotalStockValue,
                CapacityPercentage = entity.CapacityPercentage
            });
        }
    }
}
