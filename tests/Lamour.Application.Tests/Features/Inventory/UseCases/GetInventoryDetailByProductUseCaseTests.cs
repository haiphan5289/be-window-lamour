using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouse.UseCases;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Inventory.UseCases;

// 2026-10-08: Sổ chi tiết vật tư hàng hóa — Tồn chạy dần tính RIÊNG từng kho, kèm kho + đơn giá mỗi dòng.
public class GetInventoryDetailByProductUseCaseTests
{
    private readonly Mock<IInventoryRepository> _inventory = new();
    private readonly Mock<IProductRepository>   _products  = new();

    private static readonly DateOnly From = new(2026, 10, 1);
    private static readonly DateOnly To   = new(2026, 10, 31);

    private GetInventoryDetailByProductUseCase CreateSut() =>
        new(_inventory.Object, _products.Object, Mock.Of<ILogger<GetInventoryDetailByProductUseCase>>());

    private static DateTime D(int day) => new(2026, 10, day, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RunningBalance_IsPerWarehouse_AndWarehousesOpeningIsCorrect()
    {
        var product = new Product { Id = 1, Code = "SP001", Name = "Kem", Unit = "Hộp", CostPrice = 100m };
        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _inventory.Setup(r => r.GetWarehousesAsync(It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<(int Id, string Code, string Name)> { (4, "HH", "Hàng Hóa"), (5, "TB", "Trưng bày") });

        // HH: closing 20; nhập 10 (ngày 5) + xuất 3 (ngày 6) => opening = 20 - (10 - 3) = 13
        // TB: closing 4;  xuất 1 (ngày 2) + trả lại 2 (ngày 7) => opening = 4 - (2 - 1) = 3
        _inventory.Setup(r => r.GetClosingQtyByWarehouseProductAsync(It.IsAny<IReadOnlyList<int>?>(), 1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Dictionary<(int WarehouseId, int ProductId), int> { [(4, 1)] = 20, [(5, 1)] = 4 });

        _inventory.Setup(r => r.GetTransactionLinesByProductAsync(1, From, To, It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<(DateTime, DateTime, string, string, int?, string?, string, int, decimal, int, int, string, string)>
                  {
                      (D(5), D(5), "NK00001", "Import",      10, "Nhập", "Hộp", 10, 900m, 0, 4, "HH", "Hàng Hóa"),
                      (D(6), D(6), "XK00001", "Export",      20, "Xuất", "Hộp", 0,   0m,  3, 4, "HH", "Hàng Hóa"),
                      (D(2), D(2), "XK00002", "Export",      21, "Xuất", "Hộp", 0,   0m,  1, 5, "TB", "Trưng bày"),
                      (D(7), D(7), "TL00001", "SalesReturn", null, "Trả", "Hộp", 2,  0m,  0, 5, "TB", "Trưng bày"),
                  });

        var result = await CreateSut().ExecuteAsync(1, From, To);

        Assert.NotNull(result);

        // Kho sắp theo tên: Hàng Hóa, Trưng bày
        Assert.Equal(2, result!.Warehouses.Count);
        var hh = result.Warehouses[0];
        var tb = result.Warehouses[1];
        Assert.Equal((4, "HH", "Hàng Hóa", 13, 1300m, 20, 2000m),
            (hh.WarehouseId, hh.WarehouseCode, hh.WarehouseName, hh.OpeningQty, hh.OpeningValue, hh.ClosingQty, hh.ClosingValue));
        Assert.Equal((5, "TB", "Trưng bày", 3, 300m, 4, 400m),
            (tb.WarehouseId, tb.WarehouseCode, tb.WarehouseName, tb.OpeningQty, tb.OpeningValue, tb.ClosingQty, tb.ClosingValue));

        // Top-level = tổng các kho
        Assert.Equal(16, result.OpeningQty);
        Assert.Equal(1600m, result.OpeningValue);
        Assert.Equal(24, result.ClosingQty);
        Assert.Equal(2400m, result.ClosingValue);

        // Dòng: theo kho (tên) -> ngày -> số chứng từ; tồn chạy dần tách theo kho
        Assert.Equal(new[] { "NK00001", "XK00001", "XK00002", "TL00001" }, result.Lines.Select(l => l.DocumentNumber));
        Assert.Equal(new[] { 23, 20, 2, 4 }, result.Lines.Select(l => l.RunningQty));
        Assert.Equal(new[] { 2300m, 2000m, 200m, 400m }, result.Lines.Select(l => l.RunningValue));
        Assert.Equal(new[] { 4, 4, 5, 5 }, result.Lines.Select(l => l.WarehouseId));
        Assert.Equal(new[] { "HH", "HH", "TB", "TB" }, result.Lines.Select(l => l.WarehouseCode));
        Assert.Equal(new[] { "Hàng Hóa", "Hàng Hóa", "Trưng bày", "Trưng bày" }, result.Lines.Select(l => l.WarehouseName));

        // Đơn giá: nhập = giá trị thật / SL; xuất & trả lại = CostPrice
        Assert.Equal(new[] { 90m, 100m, 100m, 100m }, result.Lines.Select(l => l.UnitPrice));
    }

    [Fact]
    public async Task SelectedWarehouseWithoutActivity_StillListedWithZeroUnitPriceForNoLines()
    {
        var product = new Product { Id = 1, Code = "SP001", Name = "Kem", Unit = "Hộp", CostPrice = 100m };
        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _inventory.Setup(r => r.GetWarehousesAsync(It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<(int Id, string Code, string Name)> { (5, "TB", "Trưng bày") });
        _inventory.Setup(r => r.GetClosingQtyByWarehouseProductAsync(It.IsAny<IReadOnlyList<int>?>(), 1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Dictionary<(int WarehouseId, int ProductId), int>());
        _inventory.Setup(r => r.GetTransactionLinesByProductAsync(1, From, To, It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<(DateTime, DateTime, string, string, int?, string?, string, int, decimal, int, int, string, string)>());

        var result = await CreateSut().ExecuteAsync(1, From, To, new[] { 5 });

        var w = Assert.Single(result!.Warehouses);
        Assert.Equal(5, w.WarehouseId);
        Assert.Equal(0, w.OpeningQty);
        Assert.Empty(result.Lines);
    }

    [Fact]
    public async Task ProductNotFound_ReturnsNull()
    {
        _products.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        Assert.Null(await CreateSut().ExecuteAsync(9, From, To));
    }
}
