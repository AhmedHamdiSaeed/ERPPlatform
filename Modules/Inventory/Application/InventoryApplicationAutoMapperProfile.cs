using AutoMapper;
using ERPPlatform.Domain.Entities;

namespace ERPPlatform.Modules.Inventory.Application
{
    public class InventoryApplicationAutoMapperProfile : Profile
    {
        public InventoryApplicationAutoMapperProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateUpdateProductDto, Product>().ReverseMap();
            CreateMap<Warehouse, WarehouseDto>().ReverseMap();
            CreateMap<StockTransfer, StockTransferDto>().ReverseMap();
            CreateMap<SalesInvoice, SalesInvoiceDto>().ReverseMap();
            CreateMap<SalesQuotation, SalesQuotationDto>().ReverseMap();
            CreateMap<PurchaseOrder, PurchaseOrderDto>().ReverseMap();
        }
    }
}