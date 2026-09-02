using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class StockTransferDto : EntityDto<Guid>
    {
        public string TransferCode { get; set; } = string.Empty;
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public interface IStockTransferAppService : ICrudAppService<StockTransferDto, Guid, PagedAndSortedResultRequestDto, StockTransferDto>
    {
        Task UpdateStatusAsync(Guid id, string newStatus);
    }

    public class StockTransferAppService : CrudAppService<StockTransfer, StockTransferDto, Guid, PagedAndSortedResultRequestDto, StockTransferDto>, IStockTransferAppService
    {
        private static readonly string[] AllowedStatuses = { "Draft", "Pending Approval", "Approved", "In Transit", "Completed", "Rejected" };

        public StockTransferAppService(IRepository<StockTransfer, Guid> repository) : base(repository)
        {
        }

        public async Task UpdateStatusAsync(Guid id, string newStatus)
        {
            if (Array.IndexOf(AllowedStatuses, newStatus) < 0)
            {
                throw new AbpValidationException(
                    $"Invalid stock transfer status: {newStatus}. Allowed values: {string.Join(", ", AllowedStatuses)}.",
                    new[] { new System.ComponentModel.DataAnnotations.ValidationResult(
                        $"Invalid stock transfer status: {newStatus}", new[] { nameof(newStatus) }) });
            }

            var transfer = await Repository.GetAsync(id);
            transfer.Status = newStatus;
            await Repository.UpdateAsync(transfer);
        }

        protected override Task<StockTransfer> MapToEntityAsync(StockTransferDto createInput)
        {
            return Task.FromResult(new StockTransfer
            {
                TransferCode = createInput.TransferCode,
                SourceWarehouse = createInput.SourceWarehouse,
                DestinationWarehouse = createInput.DestinationWarehouse,
                ProductName = createInput.ProductName,
                Quantity = createInput.Quantity,
                RequestedBy = createInput.RequestedBy,
                Date = createInput.Date == default ? DateTime.UtcNow : createInput.Date,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Draft" : createInput.Status
            });
        }

        protected override Task MapToEntityAsync(StockTransferDto updateInput, StockTransfer entity)
        {
            entity.TransferCode = updateInput.TransferCode;
            entity.SourceWarehouse = updateInput.SourceWarehouse;
            entity.DestinationWarehouse = updateInput.DestinationWarehouse;
            entity.ProductName = updateInput.ProductName;
            entity.Quantity = updateInput.Quantity;
            entity.RequestedBy = updateInput.RequestedBy;
            entity.Status = updateInput.Status;
            return Task.CompletedTask;
        }

        protected override Task<StockTransferDto> MapToGetOutputDtoAsync(StockTransfer entity)
        {
            return Task.FromResult(new StockTransferDto
            {
                Id = entity.Id,
                TransferCode = entity.TransferCode,
                SourceWarehouse = entity.SourceWarehouse,
                DestinationWarehouse = entity.DestinationWarehouse,
                ProductName = entity.ProductName,
                Quantity = entity.Quantity,
                RequestedBy = entity.RequestedBy,
                Date = entity.Date,
                Status = entity.Status
            });
        }
    }
}
