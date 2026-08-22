using System;
using System.Threading.Tasks;
using ERPPlatform.Modules.Inventory.Application;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications;

/// <summary>
/// Integration tests for ProductAppService covering CRUD operations,
/// stock adjustment transitions, and status invariants.
/// </summary>
public class ProductAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
{
    private readonly IProductAppService _productService;

    public ProductAppServiceTests()
    {
        _productService = GetRequiredService<IProductAppService>();
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_Should_Persist_Product_With_Correct_Values()
    {
        var input = new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-001",
            Name = "Industrial Servo Motor",
            Category = "Electronics",
            Price = 299.99m,
            Stock = 100,
            ReorderLevel = 15,
            Unit = "pcs",
            WarehouseName = "Main Warehouse",
            SupplierName = "TechDrive Systems"
        };

        var result = await _productService.CreateAsync(input);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Sku.ShouldBe("PRD-TEST-001");
        result.Name.ShouldBe("Industrial Servo Motor");
        result.Price.ShouldBe(299.99m);
        result.Stock.ShouldBe(100);
    }

    // ─── STOCK ADJUSTMENTS & STATUS TRANSITIONS ──────────────────────────────

    [Fact]
    public async Task AdjustStockAsync_Should_Set_Status_To_In_Stock_When_Above_Reorder()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-010",
            Name = "Hydraulic Pump",
            Category = "Machinery",
            Price = 850m,
            Stock = 5,
            ReorderLevel = 10,
            Unit = "pcs"
        });

        await _productService.AdjustStockAsync(product.Id, 50);

        var updated = await _productService.GetAsync(product.Id);
        updated.Stock.ShouldBe(50);
        updated.Status.ShouldBe("In Stock");
    }

    [Fact]
    public async Task AdjustStockAsync_Should_Set_Status_To_Low_Stock_At_Reorder_Level()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-011",
            Name = "Pneumatic Valve",
            Category = "Industrial",
            Price = 45m,
            Stock = 50,
            ReorderLevel = 10,
            Unit = "pcs"
        });

        // Set stock exactly at reorder level
        await _productService.AdjustStockAsync(product.Id, 10);

        var updated = await _productService.GetAsync(product.Id);
        updated.Stock.ShouldBe(10);
        updated.Status.ShouldBe("Low Stock");
    }

    [Fact]
    public async Task AdjustStockAsync_Should_Set_Status_To_Out_Of_Stock_When_Zero()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-012",
            Name = "Bearing Assembly",
            Category = "Parts",
            Price = 22m,
            Stock = 30,
            ReorderLevel = 5,
            Unit = "pcs"
        });

        await _productService.AdjustStockAsync(product.Id, 0);

        var updated = await _productService.GetAsync(product.Id);
        updated.Stock.ShouldBe(0);
        updated.Status.ShouldBe("Out of Stock");
    }

    [Fact]
    public async Task AdjustStockAsync_Should_Set_Status_To_Low_Stock_Below_Reorder()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-013",
            Name = "Circuit Breaker",
            Category = "Electrical",
            Price = 120m,
            Stock = 100,
            ReorderLevel = 20,
            Unit = "pcs"
        });

        // Set stock below reorder level but above zero
        await _productService.AdjustStockAsync(product.Id, 8);

        var updated = await _productService.GetAsync(product.Id);
        updated.Stock.ShouldBe(8);
        updated.Status.ShouldBe("Low Stock");
    }

    // ─── READ & UPDATE ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetListAsync_Should_Return_All_Products()
    {
        await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-020",
            Name = "Digital Pressure Gauge",
            Category = "Instrumentation",
            Price = 75m,
            Stock = 200
        });
        await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-021",
            Name = "Flow Meter",
            Category = "Instrumentation",
            Price = 320m,
            Stock = 50
        });

        var list = await _productService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_Change_Price_And_Category()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-030",
            Name = "Relay Module",
            Category = "Electronics",
            Price = 18m,
            Stock = 500
        });

        var updated = await _productService.UpdateAsync(product.Id, new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-030",
            Name = "Relay Module Pro",
            Category = "Industrial Electronics",
            Price = 28.50m,
            Stock = 500
        });

        updated.Name.ShouldBe("Relay Module Pro");
        updated.Price.ShouldBe(28.50m);
        updated.Category.ShouldBe("Industrial Electronics");
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_Should_Remove_Product_From_Repository()
    {
        var product = await _productService.CreateAsync(new CreateUpdateProductDto
        {
            Sku = "PRD-TEST-040",
            Name = "Disposable Sensor Pack",
            Category = "Consumables",
            Price = 5m,
            Stock = 1000
        });

        await _productService.DeleteAsync(product.Id);

        await Should.ThrowAsync<Exception>(async () =>
            await _productService.GetAsync(product.Id));
    }
}
