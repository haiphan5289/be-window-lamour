using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Sales.UseCases;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Sales.UseCases;

public class UnconfirmSalesOrderUseCaseTests
{
    private static SalesOrder MakeOrder(SalesOrderStatus status, int quantity, Product product) => new()
    {
        Id             = 7,
        DocumentNumber = "XK00007",
        Status         = status,
        Customer       = new Customer { Id = 1, Code = "KH001", Name = "Spa" },
        Lines          = new List<SalesOrderLine>
        {
            new() { ProductId = product.Id, Product = product, WarehouseId = 4, ProductCode = product.Code,
                    ProductName = product.Name, Unit = "Cái", Quantity = quantity },
        },
    };

    private static (UnconfirmSalesOrderUseCase useCase, Mock<IProductWarehouseStockRepository> stockRepo, Mock<IUnitOfWork> uow)
        Build(SalesOrder order, Product product)
    {
        var repo = new Mock<ISalesOrderRepository>();
        repo.Setup(r => r.GetByIdTrackedAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdTrackedAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var stockRepo = new Mock<IProductWarehouseStockRepository>();
        var uow       = new Mock<IUnitOfWork>();

        var useCase = new UnconfirmSalesOrderUseCase(repo.Object, productRepo.Object, stockRepo.Object, uow.Object,
            Mock.Of<ILogger<UnconfirmSalesOrderUseCase>>());
        return (useCase, stockRepo, uow);
    }

    [Fact]
    public async Task Unconfirm_WhenCurrentStockLowerThanSoldQuantity_StillSucceedsAndAddsStockBack()
    {
        // Đã bán 100 nhưng tồn hiện tại chỉ còn 10 (đã xuất/bán tiếp) — bỏ ghi chỉ CỘNG tồn nên phải thành công.
        var product = new Product { Id = 6, Code = "SP006", Name = "Ống Xilanh Pro", Unit = "Cái", StockQuantity = 10 };
        var order   = MakeOrder(SalesOrderStatus.Normal, quantity: 100, product);
        var (useCase, stockRepo, uow) = Build(order, product);

        var result = await useCase.ExecuteAsync(order.Id);

        Assert.Equal(110, product.StockQuantity);
        Assert.Equal(SalesOrderStatus.Held, order.Status);
        stockRepo.Verify(r => r.AdjustQuantityAsync(product.Id, 4, 100, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task Unconfirm_WhenOrderNotPosted_ThrowsDomainException()
    {
        var product = new Product { Id = 6, Code = "SP006", Name = "Ống Xilanh Pro", Unit = "Cái", StockQuantity = 10 };
        var order   = MakeOrder(SalesOrderStatus.Held, quantity: 100, product);
        var (useCase, _, _) = Build(order, product);

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(order.Id));
        Assert.Equal(10, product.StockQuantity);
    }
}
