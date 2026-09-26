using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

// 2026-09-26: dùng để dựng lại tab "2. Chứng từ" khi Sửa 1 phiếu thu hàng loạt đã lưu — khác
// GetOutstandingSalesOrdersUseCase ở chỗ KHÔNG lọc theo còn nợ/khoảng ngày (đơn đã hết nợ hoặc
// ngoài khoảng ngày ban đầu vẫn phải hiện lại được).
public class GetSalesOrdersByIdsUseCaseTests
{
    private readonly Mock<IReceiptRepository> _repo = new();

    private GetSalesOrdersByIdsUseCase CreateSut() =>
        new(_repo.Object, Mock.Of<ILogger<GetSalesOrdersByIdsUseCase>>());

    [Fact]
    public async Task ReturnsOrders_EvenWhenFullyPaid()
    {
        _repo.Setup(r => r.GetSalesOrdersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[]
             {
                 (OrderId: 5, DocumentNumber: "BH00005", AccountingDate: DateTime.UtcNow, DocumentDate: DateTime.UtcNow,
                  CustomerId: 1, CustomerCode: "KH001", CustomerName: "Chị Anh Thư", Description: (string?)null,
                  GrandTotal: 8_739_900m, PaymentTerms: (string?)null, PaymentDueDate: (DateTime?)null,
                  RemainingAmount: 0m), // đã thu đủ — vẫn phải trả về để hiện lại khi Sửa
             });

        var sut    = CreateSut();
        var result = (await sut.ExecuteAsync(new[] { 5 })).ToList();

        var order = Assert.Single(result);
        Assert.Equal(5, order.SalesOrderId);
        Assert.Equal(0m, order.RemainingAmount);
        Assert.Equal(8_739_900m, order.GrandTotal);
    }

    [Fact]
    public async Task EmptyIds_StillCallsRepository_ReturnsEmpty()
    {
        _repo.Setup(r => r.GetSalesOrdersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Enumerable.Empty<(int, string, DateTime, DateTime, int, string, string, string?, decimal, string?, DateTime?, decimal)>());

        var sut    = CreateSut();
        var result = await sut.ExecuteAsync(Array.Empty<int>());

        Assert.Empty(result);
    }
}
