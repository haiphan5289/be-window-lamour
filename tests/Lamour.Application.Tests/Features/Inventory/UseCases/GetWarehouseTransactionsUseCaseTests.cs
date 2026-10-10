using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.UseCases;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Inventory.UseCases;

// 2026-10-10: danh sách "Nhập, Xuất Kho" khớp MISA — dòng xuất kho hiện số XK riêng và Tổng tiền 0; dòng nhập kho giữ nguyên.
public class GetWarehouseTransactionsUseCaseTests
{
    private readonly Mock<IWarehouseReceiptRepository> _receipts  = new();
    private readonly Mock<ISalesOrderRepository>       _orders    = new();
    private readonly Mock<IProductRepository>          _products  = new();
    private readonly Mock<IWarehouseRepository>        _warehouses = new();

    private GetWarehouseTransactionsUseCase CreateSut() => new(
        _receipts.Object, _orders.Object, _products.Object, _warehouses.Object,
        Mock.Of<ILogger<GetWarehouseTransactionsUseCase>>());

    private void Setup(IEnumerable<WarehouseReceipt> receipts, IEnumerable<SalesOrder> orders)
    {
        _receipts.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(receipts);
        _orders.Setup(r => r.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(orders);
        _products.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product>());
        _warehouses.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Warehouse>());
    }

    private static SalesOrder Order(int id, string number, string? exportNumber, decimal total) => new()
    {
        Id = id, DocumentNumber = number, ExportNumber = exportNumber, TotalAmount = total,
        AccountingDate = DateTime.UtcNow, DocumentDate = DateTime.UtcNow,
        Customer = new Customer { Name = "KH" }, Status = SalesOrderStatus.Normal,
    };

    [Fact]
    public async Task ExportRow_ShowsExportNumber_AndZeroTotal()
    {
        Setup(new List<WarehouseReceipt>(), new[] { Order(1, "BH00079", "XK00061", 17_280_000m) });

        var row = Assert.Single(await CreateSut().ExecuteAsync(null, null, null));

        Assert.Equal("Export", row.TransactionType);
        Assert.Equal("XK00061", row.DocumentNumber);
        Assert.Equal(0m, row.TotalAmount);
    }

    [Fact]
    public async Task ExportRow_WithoutExportNumber_FallsBackToDocumentNumber()
    {
        Setup(new List<WarehouseReceipt>(), new[] { Order(2, "XK00005", null, 100m) });

        var row = Assert.Single(await CreateSut().ExecuteAsync(null, null, null));

        Assert.Equal("XK00005", row.DocumentNumber);
    }

    [Fact]
    public async Task ImportRow_KeepsRealTotal()
    {
        var receipt = new WarehouseReceipt
        {
            Id = 3, ReceiptNumber = "NK00024", TotalAmount = 180_000m, Status = WarehouseReceiptStatus.Confirmed,
            AccountingDate = DateTime.UtcNow, DocumentDate = DateTime.UtcNow,
        };
        Setup(new[] { receipt }, new List<SalesOrder>());

        var row = Assert.Single(await CreateSut().ExecuteAsync(null, null, null));

        Assert.Equal("Import", row.TransactionType);
        Assert.Equal("NK00024", row.DocumentNumber);
        Assert.Equal(180_000m, row.TotalAmount);
    }
}
