using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Sales.UseCases;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Sales.UseCases;

// 2026-11-16: thiếu tồn kho không còn chặn lưu (giống MISA) — Create/Update lưu đơn ở trạng thái Treo
// (Held), Confirm giữ nguyên Held; cả 3 KHÔNG trừ kho và trả SalesOrderResponseDto.StockWarning.
public class SalesOrderInsufficientStockTests
{
    private const int WarehouseId = 4;

    private static Product MakeProduct(int id = 1, string name = "Kem dưỡng da", int stock = 3, bool isActive = true) => new()
    {
        Id = id, Code = $"SP{id:D3}", Name = name, Unit = "Hộp", StockQuantity = stock, IsActive = isActive,
    };

    private static SalesOrderLineDto MakeLineDto(int productId, int quantity) => new()
    {
        ProductId = productId, WarehouseId = WarehouseId, Unit = "Hộp", Quantity = quantity, UnitPrice = 100000,
    };

    private sealed class Fixture
    {
        public Mock<ISalesOrderRepository> Repo { get; } = new();
        public Mock<IProductRepository> ProductRepo { get; } = new();
        public Mock<IWarehouseRepository> WarehouseRepo { get; } = new();
        public Mock<IProductWarehouseStockRepository> StockRepo { get; } = new();
        public Mock<IUnitOfWork> Uow { get; } = new();

        // availableByProduct: số tồn trong kho WarehouseId mà GetQuantityAsync trả về (sau khi đã hoàn tác dòng cũ nếu có).
        public Fixture(IEnumerable<Product> products, Func<int, int> availableByProduct)
        {
            Repo.Setup(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SalesOrder o, CancellationToken _) => o);
            Repo.Setup(r => r.GetNextCodeNumberAsync("XK", It.IsAny<CancellationToken>())).ReturnsAsync(1);

            foreach (var p in products)
            {
                ProductRepo.Setup(r => r.GetByIdAsync(p.Id, It.IsAny<CancellationToken>())).ReturnsAsync(p);
                ProductRepo.Setup(r => r.GetByIdTrackedAsync(p.Id, It.IsAny<CancellationToken>())).ReturnsAsync(p);
                var id = p.Id;
                StockRepo.Setup(r => r.GetQuantityAsync(id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(availableByProduct(id));
            }

            WarehouseRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Warehouse { Id = WarehouseId, Code = "HH", Name = "Hàng hóa" });
        }

        public CreateSalesOrderUseCase Create() => new(Repo.Object, ProductRepo.Object, WarehouseRepo.Object,
            StockRepo.Object, Uow.Object, Mock.Of<ILogger<CreateSalesOrderUseCase>>());

        public UpdateSalesOrderUseCase Update() => new(Repo.Object, ProductRepo.Object, WarehouseRepo.Object,
            StockRepo.Object, Uow.Object, Mock.Of<ILogger<UpdateSalesOrderUseCase>>());

        public ConfirmSalesOrderUseCase Confirm() => new(Repo.Object, ProductRepo.Object,
            StockRepo.Object, Uow.Object, Mock.Of<ILogger<ConfirmSalesOrderUseCase>>());

        public void ExpectNoStockDeduction()
        {
            StockRepo.Verify(r => r.AdjustQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.Is<int>(d => d < 0),
                It.IsAny<CancellationToken>()), Times.Never);
            ProductRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }
    }

    private static CreateSalesOrderRequestDto MakeCreateRequest(params SalesOrderLineDto[] lines) => new()
    {
        DocumentNumber = "XK00001",
        AccountingDate = DateTime.UtcNow,
        DocumentDate   = DateTime.UtcNow,
        CustomerId     = 1,
        Lines          = lines.ToList(),
    };

    private static UpdateSalesOrderRequestDto MakeUpdateRequest(params SalesOrderLineDto[] lines) => new()
    {
        DocumentNumber = "XK00007",
        AccountingDate = DateTime.UtcNow,
        DocumentDate   = DateTime.UtcNow,
        CustomerId     = 1,
        Lines          = lines.ToList(),
    };

    private static SalesOrder MakeOrder(SalesOrderStatus status, Product product, int quantity) => new()
    {
        Id             = 7,
        DocumentNumber = "XK00007",
        Status         = status,
        Customer       = new Customer { Id = 1, Code = "KH001", Name = "Spa" },
        Lines          = new List<SalesOrderLine>
        {
            new() { ProductId = product.Id, Product = product, WarehouseId = WarehouseId, ProductCode = product.Code,
                    ProductName = product.Name, Unit = "Hộp", Quantity = quantity },
        },
    };

    private static void SetupOrder(Fixture f, SalesOrder order)
    {
        f.Repo.Setup(r => r.GetByIdTrackedAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        f.Repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
    }

    // ── 1 ── Create thiếu tồn → Held, không đụng kho, có cảnh báo, không throw
    [Fact]
    public async Task Create_InsufficientStock_SavesAsHeld_DoesNotDeductStock_AndReturnsWarning()
    {
        var short1 = MakeProduct(1, "Kem dưỡng da", stock: 3);
        var enough = MakeProduct(2, "Sữa rửa mặt", stock: 100);
        // Dòng 2 đủ tồn nhưng vì dòng 1 thiếu nên TOÀN ĐƠN không được đụng kho.
        var f = new Fixture(new[] { short1, enough }, id => id == 1 ? 3 : 100);

        var result = await f.Create().ExecuteAsync(MakeCreateRequest(MakeLineDto(1, 10), MakeLineDto(2, 5)));

        Assert.Equal((int)SalesOrderStatus.Held, result.Status);
        Assert.NotNull(result.StockWarning);
        Assert.Equal("Các sản phẩm không đủ tồn kho:\n• Kem dưỡng da: kho có 3, cần 10", result.StockWarning);
        Assert.DoesNotContain("Sữa rửa mặt", result.StockWarning);
        Assert.Equal(3, short1.StockQuantity);
        Assert.Equal(100, enough.StockQuantity);
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        f.ExpectNoStockDeduction();
        f.Repo.Verify(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        f.Uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        f.Uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── 2 ── Create đủ tồn → Normal, trừ kho, StockWarning null (hành vi cũ không đổi)
    [Fact]
    public async Task Create_SufficientStock_SavesAsNormal_DeductsStock_NoWarning()
    {
        var product = MakeProduct(1, stock: 100);
        var f = new Fixture(new[] { product }, _ => 100);

        var result = await f.Create().ExecuteAsync(MakeCreateRequest(MakeLineDto(1, 10)));

        Assert.Equal((int)SalesOrderStatus.Normal, result.Status);
        Assert.Null(result.StockWarning);
        Assert.Equal(90, product.StockQuantity);
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(1, WarehouseId, -10, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── 3 ── Update đơn Normal, dòng mới thiếu tồn → hoàn kho cũ, Held, không trừ kho mới
    [Fact]
    public async Task Update_NormalOrder_NewLineInsufficient_RestoresOldStock_SavesAsHeld_NoDeduction()
    {
        // Đơn Normal cũ đã bán 5 (tồn sản phẩm 3 sau khi trừ). Sửa thành 10, kho (sau hoàn 5) chỉ có 8 → thiếu.
        var product = MakeProduct(1, "Kem dưỡng da", stock: 3);
        var order   = MakeOrder(SalesOrderStatus.Normal, product, quantity: 5);
        var f = new Fixture(new[] { product }, _ => 8);
        SetupOrder(f, order);

        var result = await f.Update().ExecuteAsync(order.Id, MakeUpdateRequest(MakeLineDto(1, 10)));

        Assert.Equal((int)SalesOrderStatus.Held, result.Status);
        Assert.Equal(SalesOrderStatus.Held, order.Status);
        Assert.Equal("Các sản phẩm không đủ tồn kho:\n• Kem dưỡng da: kho có 8, cần 10", result.StockWarning);
        Assert.Equal(8, product.StockQuantity); // 3 + 5 hoàn kho dòng cũ, KHÔNG trừ dòng mới
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(1, WarehouseId, 5, It.IsAny<CancellationToken>()), Times.Once);
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.Is<int>(d => d < 0),
            It.IsAny<CancellationToken>()), Times.Never);
        f.Uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_SufficientStock_SavesAsNormal_NoWarning()
    {
        var product = MakeProduct(1, stock: 3);
        var order   = MakeOrder(SalesOrderStatus.Normal, product, quantity: 5);
        var f = new Fixture(new[] { product }, _ => 8);
        SetupOrder(f, order);

        var result = await f.Update().ExecuteAsync(order.Id, MakeUpdateRequest(MakeLineDto(1, 6)));

        Assert.Equal((int)SalesOrderStatus.Normal, result.Status);
        Assert.Null(result.StockWarning);
        Assert.Equal(2, product.StockQuantity); // 3 + 5 - 6
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(1, WarehouseId, -6, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── 4 ── Confirm đơn Held thiếu tồn → vẫn Held, không trừ kho, có cảnh báo
    [Theory]
    [InlineData(SalesOrderStatus.Held)]
    [InlineData(SalesOrderStatus.Draft)] // Draft/Held đã gộp thành "Treo" → chuẩn hoá về Held
    public async Task Confirm_InsufficientStock_KeepsHeld_DoesNotDeduct_ReturnsWarning(SalesOrderStatus initial)
    {
        var product = MakeProduct(1, "Kem dưỡng da", stock: 3);
        var order   = MakeOrder(initial, product, quantity: 10);
        var f = new Fixture(new[] { product }, _ => 3);
        SetupOrder(f, order);

        var result = await f.Confirm().ExecuteAsync(order.Id);

        Assert.Equal(SalesOrderStatus.Held, order.Status);
        Assert.Equal((int)SalesOrderStatus.Held, result.Status);
        Assert.Equal("Các sản phẩm không đủ tồn kho:\n• Kem dưỡng da: kho có 3, cần 10", result.StockWarning);
        Assert.Equal(3, product.StockQuantity);
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        f.ExpectNoStockDeduction();
        f.Uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Confirm_SufficientStock_BecomesNormal_DeductsStock_NoWarning()
    {
        var product = MakeProduct(1, stock: 50);
        var order   = MakeOrder(SalesOrderStatus.Held, product, quantity: 10);
        var f = new Fixture(new[] { product }, _ => 50);
        SetupOrder(f, order);

        var result = await f.Confirm().ExecuteAsync(order.Id);

        Assert.Equal(SalesOrderStatus.Normal, order.Status);
        Assert.Null(result.StockWarning);
        Assert.Equal(40, product.StockQuantity);
        f.StockRepo.Verify(r => r.AdjustQuantityAsync(1, WarehouseId, -10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_AlreadyNormal_StillThrowsDomainException()
    {
        var product = MakeProduct(1, stock: 50);
        var order   = MakeOrder(SalesOrderStatus.Normal, product, quantity: 10);
        var f = new Fixture(new[] { product }, _ => 50);
        SetupOrder(f, order);

        await Assert.ThrowsAsync<DomainException>(() => f.Confirm().ExecuteAsync(order.Id));
    }

    // ── 5 ── Lỗi khác vẫn throw 400
    [Fact]
    public async Task Create_InactiveProduct_StillThrowsDomainException()
    {
        var inactive = MakeProduct(1, isActive: false);
        var f = new Fixture(new[] { inactive }, _ => 0);

        await Assert.ThrowsAsync<DomainException>(() => f.Create().ExecuteAsync(MakeCreateRequest(MakeLineDto(1, 1))));
        f.Repo.Verify(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_UnknownProduct_StillThrowsDomainException()
    {
        var f = new Fixture(Array.Empty<Product>(), _ => 0);

        await Assert.ThrowsAsync<DomainException>(() => f.Create().ExecuteAsync(MakeCreateRequest(MakeLineDto(99, 1))));
        f.Repo.Verify(r => r.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_MissingWarehouse_StillThrowsDomainException()
    {
        var product = MakeProduct(1, stock: 0);
        var f = new Fixture(new[] { product }, _ => 0);
        f.WarehouseRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        await Assert.ThrowsAsync<DomainException>(() => f.Create().ExecuteAsync(MakeCreateRequest(MakeLineDto(1, 1))));
    }
}
