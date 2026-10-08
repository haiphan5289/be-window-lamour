using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouse.UseCases;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Inventory.UseCases;

// 2026-10-08: Tổng hợp tồn kho chia theo kho — số liệu tính riêng từng cặp (kho, sản phẩm).
public class GetInventorySummaryByWarehouseUseCaseTests
{
    private readonly Mock<IInventoryRepository> _repo = new();

    private static readonly DateOnly From = new(2026, 10, 1);
    private static readonly DateOnly To   = new(2026, 10, 31);

    private GetInventorySummaryByWarehouseUseCase CreateSut() =>
        new(_repo.Object, Mock.Of<ILogger<GetInventorySummaryByWarehouseUseCase>>());

    private void Setup(
        IEnumerable<Product> products,
        List<(int Id, string Code, string Name)> warehouses,
        Dictionary<(int, int), (int Qty, decimal Value, DateTime? LatestDate)> imports,
        Dictionary<(int, int), int> exports,
        Dictionary<(int, int), int> closing)
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);
        _repo.Setup(r => r.GetWarehousesAsync(It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(warehouses);
        _repo.Setup(r => r.GetImportsByWarehouseProductAsync(From, To, It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(imports.ToDictionary(kv => (kv.Key.Item1, kv.Key.Item2), kv => kv.Value));
        _repo.Setup(r => r.GetExportQtyByWarehouseProductAsync(From, To, It.IsAny<IReadOnlyList<int>?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(exports.ToDictionary(kv => (kv.Key.Item1, kv.Key.Item2), kv => kv.Value));
        _repo.Setup(r => r.GetClosingQtyByWarehouseProductAsync(It.IsAny<IReadOnlyList<int>?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(closing.ToDictionary(kv => (kv.Key.Item1, kv.Key.Item2), kv => kv.Value));
    }

    [Fact]
    public async Task SameProductInTwoWarehouses_ReturnsTwoGroupsWithOwnNumbers_AndOmitsAllZeroRows()
    {
        var p1 = new Product { Id = 1, Code = "SP001", Name = "Kem", Unit = "Hộp", CostPrice = 100m };
        var p2 = new Product { Id = 2, Code = "SP002", Name = "Sữa", Unit = "Chai", CostPrice = 50m };
        var importDate = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

        Setup(
            new[] { p1, p2 },
            new() { (5, "TB", "Trưng bày"), (4, "HH", "Hàng Hóa") },
            imports: new()
            {
                [(4, 1)] = (10, 900m, importDate),   // HH: nhập 10 SP001 giá trị thật 900
            },
            exports: new()
            {
                [(4, 1)] = 3,                         // HH: xuất 3
                [(5, 1)] = 1,                         // TB: xuất 1
            },
            closing: new()
            {
                [(4, 1)] = 20,                        // HH: tồn cuối 20 => đầu = 20 - 10 + 3 = 13
                [(5, 1)] = 4,                         // TB: tồn cuối 4  => đầu = 4 - 0 + 1 = 5
                // SP002: không có số liệu ở cả 2 kho => bị loại
            });

        var result = (await CreateSut().ExecuteAsync(From, To)).ToList();

        Assert.Equal(2, result.Count);
        // Sắp theo tên kho tăng dần: "Hàng Hóa" < "Trưng bày"
        Assert.Equal(4, result[0].WarehouseId);
        Assert.Equal("HH", result[0].WarehouseCode);
        Assert.Equal("Hàng Hóa", result[0].WarehouseName);
        Assert.Equal(5, result[1].WarehouseId);

        var hh = Assert.Single(result[0].Items);
        Assert.Equal(1, hh.ProductId);
        Assert.Equal(13, hh.OpeningQty);
        Assert.Equal(1300m, hh.OpeningValue);
        Assert.Equal(10, hh.ImportQty);
        Assert.Equal(900m, hh.ImportValue);
        Assert.Equal(3, hh.ExportQty);
        Assert.Equal(300m, hh.ExportValue);
        Assert.Equal(20, hh.ClosingQty);
        Assert.Equal(2000m, hh.ClosingValue);
        Assert.Equal(importDate, hh.LatestAccountingDate);

        var tb = Assert.Single(result[1].Items);
        Assert.Equal(5, tb.OpeningQty);
        Assert.Equal(0, tb.ImportQty);
        Assert.Equal(0m, tb.ImportValue);
        Assert.Equal(1, tb.ExportQty);
        Assert.Equal(4, tb.ClosingQty);
        Assert.Null(tb.LatestAccountingDate);
    }

    [Fact]
    public async Task WarehouseWithNoActivity_IsOmitted()
    {
        var p1 = new Product { Id = 1, Code = "SP001", Name = "Kem", Unit = "Hộp", CostPrice = 100m };

        Setup(
            new[] { p1 },
            new() { (4, "HH", "Hàng Hóa"), (5, "TB", "Trưng bày") },
            imports: new(),
            exports: new(),
            closing: new() { [(4, 1)] = 7 });

        var result = (await CreateSut().ExecuteAsync(From, To)).ToList();

        var group = Assert.Single(result);
        Assert.Equal(4, group.WarehouseId);
    }
}
