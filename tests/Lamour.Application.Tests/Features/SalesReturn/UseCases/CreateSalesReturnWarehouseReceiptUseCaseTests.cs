using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.SalesReturn.UseCases;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.SalesReturn.UseCases;

// 2026-11-15: đổi khách ở chứng từ hàng bán bị trả lại (Kathy Spa → Spa Kim Lợi) rồi Cất, bản in Phiếu nhập
// kho vẫn ra tên khách cũ vì PN đã lập trước đó không được đồng bộ lại CustomerId (và còn giữ
// DeliveryPerson = tên khách cũ từ các PN lập bằng code cũ).
public class CreateSalesReturnWarehouseReceiptUseCaseTests
{
    private const int OldCustomerId = 45;   // Kathy Spa
    private const int NewCustomerId = 68;   // Spa Kim Lợi

    private readonly Mock<ISalesReturnRepository>      _returnRepo  = new();
    private readonly Mock<IWarehouseReceiptRepository> _receiptRepo = new();

    private CreateSalesReturnWarehouseReceiptUseCase CreateSut() =>
        new(_returnRepo.Object, _receiptRepo.Object, Mock.Of<ILogger<CreateSalesReturnWarehouseReceiptUseCase>>());

    private static Domain.Entities.SalesReturn ReturnFor(int customerId, int quantity) => new()
    {
        Id             = 19,
        DocumentNumber = "BTL00019",
        CustomerId     = customerId,
        Customer       = new Customer { Id = customerId, Name = "KH" },
        EmployeeId     = 18,
        AccountingDate = new DateTime(2026, 11, 13),
        DocumentDate   = new DateTime(2026, 11, 13),
        Description    = "Thu hồi lại hàng",
        Lines =
        {
            new SalesReturnLine { ProductId = 28, WarehouseId = 1, Quantity = quantity, CostPrice = 100, CostAmount = quantity * 100m },
        },
    };

    private static WarehouseReceipt ExistingReceiptFor(int customerId, int quantity) => new()
    {
        Id             = 29,
        ReceiptNumber  = "NK00029",
        ReceiptType    = WarehouseReceiptType.ReturnedGoods,
        Status         = WarehouseReceiptStatus.Confirmed,
        Reference      = "BTL00019",
        CustomerId     = customerId,
        DeliveryPerson = "Kathy Spa", // còn sót từ PN lập bằng code cũ
        Lines =
        {
            new WarehouseReceiptLine { ProductId = 28, WarehouseId = 1, Quantity = quantity },
        },
    };

    private void Arrange(Domain.Entities.SalesReturn salesReturn, WarehouseReceipt receipt)
    {
        _returnRepo.Setup(r => r.GetByIdAsync(salesReturn.Id, It.IsAny<CancellationToken>())).ReturnsAsync(salesReturn);
        _receiptRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WarehouseReceipt> { receipt });
        _receiptRepo.Setup(r => r.GetByIdAsync(receipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
    }

    [Fact]
    public async Task CustomerChanged_LinesUnchanged_ReceiptIsSyncedToNewCustomer_AndStaleDeliveryPersonCleared()
    {
        var receipt = ExistingReceiptFor(OldCustomerId, quantity: 5);
        Arrange(ReturnFor(NewCustomerId, quantity: 5), receipt);

        await CreateSut().ExecuteAsync(19);

        Assert.Equal(NewCustomerId, receipt.CustomerId);
        Assert.Equal(18, receipt.EmployeeId);
        Assert.Null(receipt.DeliveryPerson);
        Assert.Equal("NK00029", receipt.ReceiptNumber); // số PN giữ nguyên
        _receiptRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _receiptRepo.Verify(r => r.AddAsync(It.IsAny<WarehouseReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CustomerChanged_LinesAlsoChanged_ReceiptIsSyncedToNewCustomer_AndLinesReplaced()
    {
        var receipt = ExistingReceiptFor(OldCustomerId, quantity: 5);
        Arrange(ReturnFor(NewCustomerId, quantity: 8), receipt);

        await CreateSut().ExecuteAsync(19);

        Assert.Equal(NewCustomerId, receipt.CustomerId);
        Assert.Null(receipt.DeliveryPerson);
        var line = Assert.Single(receipt.Lines);
        Assert.Equal(8, line.Quantity);
        Assert.Equal(800m, receipt.TotalAmount);
    }

    [Fact]
    public async Task NothingChanged_LinesAreNotRewritten()
    {
        var receipt = ExistingReceiptFor(NewCustomerId, quantity: 5);
        var originalLine = receipt.Lines.Single();
        Arrange(ReturnFor(NewCustomerId, quantity: 5), receipt);

        await CreateSut().ExecuteAsync(19);

        Assert.Same(originalLine, receipt.Lines.Single()); // dòng không bị xoá-tạo lại khi không đổi
        Assert.Equal(NewCustomerId, receipt.CustomerId);
    }
}
