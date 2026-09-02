using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class PurchaseOrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class PurchaseOrderDto : EntityDto<Guid>
    {
        public string PoNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending Approval";
    }

    public interface IPurchaseOrderAppService : ICrudAppService<PurchaseOrderDto, Guid, PagedAndSortedResultRequestDto, PurchaseOrderDto>
    {
        Task UpdateStatusAsync(Guid id, string newStatus);
    }

    public class PurchaseOrderAppService : CrudAppService<PurchaseOrder, PurchaseOrderDto, Guid, PagedAndSortedResultRequestDto, PurchaseOrderDto>, IPurchaseOrderAppService
    {
        private const decimal TaxRate = 0.14m;

        private static readonly string[] AllowedStatuses = { "Draft", "Pending Approval", "Approved", "Received", "Cancelled" };

        public PurchaseOrderAppService(IRepository<PurchaseOrder, Guid> repository) : base(repository)
        {
        }

        public async Task UpdateStatusAsync(Guid id, string newStatus)
        {
            if (Array.IndexOf(AllowedStatuses, newStatus) < 0)
            {
                throw new AbpValidationException(
                    $"Invalid purchase order status: {newStatus}. Allowed values: {string.Join(", ", AllowedStatuses)}.",
                    new[] { new System.ComponentModel.DataAnnotations.ValidationResult(
                        $"Invalid purchase order status: {newStatus}", new[] { nameof(newStatus) }) });
            }

            var order = await Repository.GetAsync(id);
            order.Status = newStatus;
            await Repository.UpdateAsync(order);
        }

        protected override Task<PurchaseOrder> MapToEntityAsync(PurchaseOrderDto createInput)
        {
            var subtotal = createInput.Items.Sum(i => i.Quantity * i.UnitPrice);
            var tax = Math.Round(subtotal * TaxRate, 2);
            var discount = createInput.Discount;

            return Task.FromResult(new PurchaseOrder
            {
                PoNumber = string.IsNullOrWhiteSpace(createInput.PoNumber) ? $"PO-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}" : createInput.PoNumber,
                SupplierName = createInput.SupplierName,
                OrderDate = createInput.OrderDate == default ? DateTime.UtcNow : createInput.OrderDate,
                DeliveryDate = createInput.DeliveryDate,
                Subtotal = subtotal,
                Tax = tax,
                Discount = discount,
                GrandTotal = subtotal + tax - discount,
                CreatedBy = createInput.CreatedBy,
                Status = string.IsNullOrWhiteSpace(createInput.Status) ? "Pending Approval" : createInput.Status
            });
        }

        protected override Task MapToEntityAsync(PurchaseOrderDto updateInput, PurchaseOrder entity)
        {
            var subtotal = updateInput.Items.Sum(i => i.Quantity * i.UnitPrice);
            var tax = Math.Round(subtotal * TaxRate, 2);

            entity.PoNumber = updateInput.PoNumber;
            entity.SupplierName = updateInput.SupplierName;
            entity.DeliveryDate = updateInput.DeliveryDate;
            entity.Subtotal = subtotal;
            entity.Tax = tax;
            entity.Discount = updateInput.Discount;
            entity.GrandTotal = subtotal + tax - updateInput.Discount;
            entity.CreatedBy = updateInput.CreatedBy;
            entity.Status = updateInput.Status;
            return Task.CompletedTask;
        }

        protected override Task<PurchaseOrderDto> MapToGetOutputDtoAsync(PurchaseOrder entity)
        {
            return Task.FromResult(new PurchaseOrderDto
            {
                Id = entity.Id,
                PoNumber = entity.PoNumber,
                SupplierName = entity.SupplierName,
                OrderDate = entity.OrderDate,
                DeliveryDate = entity.DeliveryDate,
                Items = new List<PurchaseOrderItemDto>(),
                Subtotal = entity.Subtotal,
                Tax = entity.Tax,
                Discount = entity.Discount,
                GrandTotal = entity.GrandTotal,
                CreatedBy = entity.CreatedBy,
                Status = entity.Status
            });
        }
    }
}
