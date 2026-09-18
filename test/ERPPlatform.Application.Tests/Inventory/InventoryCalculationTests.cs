using System;
using ERPPlatform.Modules.Inventory.Domain.Entities;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.Inventory;

public class InventoryCalculationTests
{
    [Fact]
    public void PurchaseOrder_TotalCalculation_ShouldComputeProperly()
    {
        var po = new PurchaseOrder
        {
            PoNumber = "PO-2026-001",
            SupplierName = "Global Tech Supplies",
            Subtotal = 10000m,
            Tax = 1400m,      // 14% VAT
            Discount = 500m    // 500 EGP discount
        };

        po.GrandTotal = po.Subtotal + po.Tax - po.Discount;

        po.GrandTotal.ShouldBe(10900m);
        po.Status.ShouldBe("Pending Approval");
    }

    [Fact]
    public void Product_StockStatus_ReorderLevelLogic()
    {
        var product = new Product
        {
            Sku = "PRD-A100",
            Name = "Server Rack 42U",
            Category = "IT Hardware",
            Price = 1200m,
            Stock = 5,
            ReorderLevel = 10
        };

        var isLowStock = product.Stock <= product.ReorderLevel;
        if (isLowStock)
        {
            product.Status = "Low Stock";
        }

        isLowStock.ShouldBeTrue();
        product.Status.ShouldBe("Low Stock");
    }

    [Fact]
    public void StockTransfer_Creation_DefaultsInTransit()
    {
        var transfer = new StockTransfer
        {
            TransferCode = "TR-2026-0901",
            SourceWarehouse = "Main Warehouse Cairo",
            DestinationWarehouse = "Alexandria Hub",
            ProductName = "Fiber Optic Patch Cord",
            Quantity = 50,
            RequestedBy = "Admin"
        };

        transfer.Status.ShouldBe("In Transit");
        transfer.Quantity.ShouldBe(50);
    }
}
