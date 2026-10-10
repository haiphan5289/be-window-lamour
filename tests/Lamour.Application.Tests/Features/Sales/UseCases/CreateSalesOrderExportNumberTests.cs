using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Sales.UseCases;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Sales.UseCases;

// 2026-10-10: mỗi lần xuất kho có số XK riêng hiện ở màn Kho — đơn tạo từ Kho dùng luôn số XK của nó,
// đơn BH (tạo từ module Bán hàng) được cấp thêm 1 số XK kế tiếp.
public class CreateSalesOrderExportNumberTests
{
    private static async Task<SalesOrder> CreateAsync(string documentNumber, int nextXk)
    {
        var product = new Product { Id = 1, Code = "SP001", Name = "Kem", Unit = "Hộp", StockQuantity = 100, IsActive = true };

        var repo = new Mock<ISalesOrderRepository>();
        repo.Setup(r => r.GetNextCodeNumberAsync("XK", It.IsAny<CancellationToken>())).ReturnsAsync(nextXk);
        SalesOrder? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
            .Callback((SalesOrder o, CancellationToken _) => added = o)
            .ReturnsAsync((SalesOrder o, CancellationToken _) => o);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        productRepo.Setup(r => r.GetByIdTrackedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var warehouseRepo = new Mock<IWarehouseRepository>();
        warehouseRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Warehouse { Id = 0, Code = "HH", Name = "Hàng hóa" });

        var stockRepo = new Mock<IProductWarehouseStockRepository>();
        stockRepo.Setup(r => r.GetQuantityAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(100);

        var useCase = new CreateSalesOrderUseCase(repo.Object, productRepo.Object, warehouseRepo.Object,
            stockRepo.Object, new Mock<IUnitOfWork>().Object, Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        await useCase.ExecuteAsync(new CreateSalesOrderRequestDto
        {
            DocumentNumber = documentNumber,
            AccountingDate = DateTime.UtcNow,
            DocumentDate   = DateTime.UtcNow,
            CustomerId     = 1,
            Lines = new List<SalesOrderLineDto>
            {
                new() { ProductId = 1, Unit = "Hộp", Quantity = 1, UnitPrice = 100, Amount = 100 },
            },
        });

        return added!;
    }

    [Fact]
    public async Task DirectOrder_GetsNextExportNumber()
    {
        var order = await CreateAsync("BH00080", nextXk: 62);

        Assert.Equal("BH00080", order.DocumentNumber);
        Assert.Equal("XK00062", order.ExportNumber);
    }

    [Fact]
    public async Task WarehouseOrder_ReusesItsOwnXkNumber()
    {
        var order = await CreateAsync("XK00061", nextXk: 62);

        Assert.Equal("XK00061", order.ExportNumber);
    }
}
