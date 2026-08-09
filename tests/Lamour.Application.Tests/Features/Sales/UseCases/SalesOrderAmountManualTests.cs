using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Sales.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Sales.UseCases;

public class SalesOrderAmountManualTests
{
    private static Product MakeProduct(int id = 1, int stock = 100) => new()
    {
        Id            = id,
        Code          = "SP001",
        Name          = "Kem dưỡng da",
        Unit          = "Hộp",
        StockQuantity = stock,
        IsActive      = true,
    };

    private static SalesOrderLineDto MakeLineDto(
        bool isAmountManual, decimal amount, decimal unitPrice = 150000, decimal discountRate = 10,
        int quantity = 2, bool isPromotion = false) => new()
    {
        ProductId      = 1,
        Unit           = "Hộp",
        Quantity       = quantity,
        UnitPrice      = unitPrice,
        DiscountRate   = discountRate,
        Amount         = amount,
        IsAmountManual = isAmountManual,
        IsPromotion    = isPromotion,
    };

    private static CreateSalesOrderRequestDto MakeCreateRequest(SalesOrderLineDto line) => new()
    {
        DocumentNumber = "BC00001",
        AccountingDate = DateTime.UtcNow,
        DocumentDate   = DateTime.UtcNow,
        CustomerId     = 1,
        Lines          = new List<SalesOrderLineDto> { line },
    };

    private static (Mock<ISalesOrderRepository> repo, Mock<IProductRepository> productRepo, Mock<IUnitOfWork> uow)
        MakeMocks(Product product)
    {
        var repo = new Mock<ISalesOrderRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesOrder o, CancellationToken _) => o);

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        productRepo.Setup(r => r.GetByIdTrackedAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var uow = new Mock<IUnitOfWork>();
        return (repo, productRepo, uow);
    }

    [Fact]
    public async Task Create_WithAmountManual_UsesClientAmount_IgnoringFormula()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);
        var useCase = new CreateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        // Formula would give 2 * 150000 * (1 - 10/100) = 270000, but manual amount overrides it.
        var line = MakeLineDto(isAmountManual: true, amount: 999000);
        var request = MakeCreateRequest(line);

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal(999000, result.Lines.Single().Amount);
        Assert.True(result.Lines.Single().IsAmountManual);
    }

    [Fact]
    public async Task Create_WithAmountManualNegative_ThrowsDomainException()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);
        var useCase = new CreateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        var line = MakeLineDto(isAmountManual: true, amount: -1000);
        var request = MakeCreateRequest(line);

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Create_WithAmountManualFalse_KeepsAutoCalcFormula()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);
        var useCase = new CreateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        // amount field sent by client is ignored when IsAmountManual = false.
        var line = MakeLineDto(isAmountManual: false, amount: 999000, unitPrice: 150000, discountRate: 10, quantity: 2);
        var request = MakeCreateRequest(line);

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal(270000, result.Lines.Single().Amount);
        Assert.False(result.Lines.Single().IsAmountManual);
    }

    [Fact]
    public async Task Create_PromotionLine_IgnoresAmountManual_ForcesZero()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);
        var useCase = new CreateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        var line = MakeLineDto(isAmountManual: true, amount: 999000, isPromotion: true);
        var request = MakeCreateRequest(line);

        var result = await useCase.ExecuteAsync(request);

        Assert.Equal(0, result.Lines.Single().Amount);
        Assert.False(result.Lines.Single().IsAmountManual);
    }

    [Fact]
    public async Task Update_WithAmountManual_UsesClientAmount_IgnoringFormula()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);

        var existingOrder = new SalesOrder
        {
            Id     = 1,
            Lines  = new List<SalesOrderLine>(),
        };
        repo.Setup(r => r.GetByIdTrackedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingOrder);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingOrder);

        var useCase = new UpdateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<UpdateSalesOrderUseCase>>());

        var line = MakeLineDto(isAmountManual: true, amount: 555000);
        var request = new UpdateSalesOrderRequestDto
        {
            DocumentNumber = "BC00001",
            AccountingDate = DateTime.UtcNow,
            DocumentDate   = DateTime.UtcNow,
            CustomerId     = 1,
            Lines          = new List<SalesOrderLineDto> { line },
        };

        var result = await useCase.ExecuteAsync(1, request);

        Assert.Equal(555000, result.Lines.Single().Amount);
        Assert.True(result.Lines.Single().IsAmountManual);
    }

    [Fact]
    public async Task Update_WithAmountManualNegative_ThrowsDomainException()
    {
        var product = MakeProduct();
        var (repo, productRepo, uow) = MakeMocks(product);

        var existingOrder = new SalesOrder { Id = 1, Lines = new List<SalesOrderLine>() };
        repo.Setup(r => r.GetByIdTrackedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingOrder);

        var useCase = new UpdateSalesOrderUseCase(repo.Object, productRepo.Object, uow.Object,
            Mock.Of<ILogger<UpdateSalesOrderUseCase>>());

        var line = MakeLineDto(isAmountManual: true, amount: -50);
        var request = new UpdateSalesOrderRequestDto
        {
            DocumentNumber = "BC00001",
            AccountingDate = DateTime.UtcNow,
            DocumentDate   = DateTime.UtcNow,
            CustomerId     = 1,
            Lines          = new List<SalesOrderLineDto> { line },
        };

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(1, request));
    }
}
