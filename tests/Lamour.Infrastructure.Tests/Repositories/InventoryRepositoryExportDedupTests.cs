using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Lamour.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lamour.Infrastructure.Tests.Repositories;

// 2026-10-17 — Khớp MISA: chứng từ hàng bán bị trả lại ĐÃ "Lập PN" (có WarehouseReceipt
// ReceiptType=ReturnedGoods đang hiệu lực) được GetImportsByProductAsync tính vào Nhập kho rồi,
// nên không được trừ lại lần 2 ở Xuất kho (bug cũ: Tổng hợp tồn kho hiện "Xuất kho (3)" âm thay vì
// 0 vì trừ 2 lần cùng 1 lần trả hàng). Các test này dựng trực tiếp InventoryRepository trên EF Core
// InMemory provider (không mock IInventoryRepository) vì logic de-dup nằm NGAY TRONG câu LINQ của
// repository — test ở tầng UseCase (mock repository) sẽ không chạm được bug này.
public class InventoryRepositoryExportDedupTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InventoryRepository _sut;

    private static readonly DateOnly From = new(2026, 10, 1);
    private static readonly DateOnly To   = new(2026, 10, 31);

    public InventoryRepositoryExportDedupTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db  = new AppDbContext(options);
        _sut = new InventoryRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    private static DateTime Mid => new(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);

    // Đơn bán 10 SP001 qua kho HH trong kỳ, sau đó khách trả lại 3.
    private (SalesOrder order, SalesReturn ret) SeedSaleAndReturn(int productId, int warehouseId, int soldQty, int returnedQty)
    {
        var order = new SalesOrder
        {
            DocumentNumber = "BH-001",
            AccountingDate = Mid,
            DocumentDate   = Mid,
            CustomerId     = 1,
            Lines = new List<SalesOrderLine>
            {
                new() { ProductId = productId, WarehouseId = warehouseId, Quantity = soldQty, IsPromotion = false },
            },
        };
        _db.SalesOrders.Add(order);

        var ret = new SalesReturn
        {
            DocumentNumber = "BTL-001",
            AccountingDate = Mid,
            DocumentDate   = Mid,
            CustomerId     = 1,
            Lines = new List<SalesReturnLine>
            {
                new() { ProductId = productId, WarehouseId = warehouseId, Quantity = returnedQty },
            },
        };
        _db.SalesReturns.Add(ret);

        return (order, ret);
    }

    private void SeedActivePn(string reference)
    {
        _db.WarehouseReceipts.Add(new WarehouseReceipt
        {
            ReceiptNumber  = "NK-001",
            ReceiptType    = WarehouseReceiptType.ReturnedGoods,
            Status         = WarehouseReceiptStatus.Confirmed,
            IsSuperseded   = false,
            Reference      = reference,
            AccountingDate = Mid,
            DocumentDate   = Mid,
        });
    }

    [Fact]
    public async Task GetExportQtyByProductAsync_ReturnAlreadyHasActivePn_DoesNotDoubleCount()
    {
        var (_, ret) = SeedSaleAndReturn(productId: 1, warehouseId: 4, soldQty: 10, returnedQty: 3);
        SeedActivePn(ret.DocumentNumber);
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByProductAsync(From, To);

        // Đã "Lập PN" (Nhập kho đã phản ánh 3 cái trả lại) → Xuất kho giữ đúng 10, không bị trừ thêm.
        Assert.Equal(10, result[1]);
    }

    [Fact]
    public async Task GetExportQtyByProductAsync_ReturnWithoutPn_StillSubtractsFromExport()
    {
        var (_, _) = SeedSaleAndReturn(productId: 1, warehouseId: 4, soldQty: 10, returnedQty: 3);
        // Không lập PN cho chứng từ trả hàng này.
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByProductAsync(From, To);

        Assert.Equal(7, result[1]);
    }

    [Fact]
    public async Task GetExportQtyByProductAsync_SupersededPn_IsIgnored_StillSubtracts()
    {
        var (_, ret) = SeedSaleAndReturn(productId: 1, warehouseId: 4, soldQty: 10, returnedQty: 3);
        _db.WarehouseReceipts.Add(new WarehouseReceipt
        {
            ReceiptNumber  = "NK-001",
            ReceiptType    = WarehouseReceiptType.ReturnedGoods,
            Status         = WarehouseReceiptStatus.Confirmed,
            IsSuperseded   = true, // PN cũ đã bị thay thế — không còn hiệu lực
            Reference      = ret.DocumentNumber,
            AccountingDate = Mid,
            DocumentDate   = Mid,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByProductAsync(From, To);

        Assert.Equal(7, result[1]);
    }

    [Fact]
    public async Task GetExportQtyByWarehouseProductAsync_ReturnAlreadyHasActivePn_DoesNotDoubleCount()
    {
        var (_, ret) = SeedSaleAndReturn(productId: 1, warehouseId: 4, soldQty: 10, returnedQty: 3);
        SeedActivePn(ret.DocumentNumber);
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByWarehouseProductAsync(From, To);

        Assert.Equal(10, result[(4, 1)]);
    }

    [Fact]
    public async Task GetExportQtyByWarehouseProductAsync_ReturnWithoutPn_StillSubtractsFromExport()
    {
        var (_, _) = SeedSaleAndReturn(productId: 1, warehouseId: 4, soldQty: 10, returnedQty: 3);
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByWarehouseProductAsync(From, To);

        Assert.Equal(7, result[(4, 1)]);
    }

    [Fact]
    public async Task GetExportQtyByWarehouseProductAsync_PnForDifferentWarehouse_DoesNotAffectOtherWarehouse()
    {
        // PN "Lập" chỉ khớp theo DocumentNumber của CHỨNG TỪ (không theo kho) — nhưng 2 kho khác nhau
        // của CÙNG 1 sản phẩm phải vẫn tách riêng số liệu đúng theo dòng hàng của từng kho.
        var order = new SalesOrder
        {
            DocumentNumber = "BH-002", AccountingDate = Mid, DocumentDate = Mid, CustomerId = 1,
            Lines = new List<SalesOrderLine>
            {
                new() { ProductId = 1, WarehouseId = 4, Quantity = 10, IsPromotion = false },
                new() { ProductId = 1, WarehouseId = 5, Quantity = 6,  IsPromotion = false },
            },
        };
        _db.SalesOrders.Add(order);

        var ret = new SalesReturn
        {
            DocumentNumber = "BTL-002", AccountingDate = Mid, DocumentDate = Mid, CustomerId = 1,
            Lines = new List<SalesReturnLine>
            {
                new() { ProductId = 1, WarehouseId = 4, Quantity = 3 }, // chỉ trả hàng ở kho 4
            },
        };
        _db.SalesReturns.Add(ret);
        SeedActivePn(ret.DocumentNumber);
        await _db.SaveChangesAsync();

        var result = await _sut.GetExportQtyByWarehouseProductAsync(From, To);

        Assert.Equal(10, result[(4, 1)]); // đã lập PN, không trừ
        Assert.Equal(6, result[(5, 1)]);  // kho 5 không liên quan, không bị ảnh hưởng
    }
}
