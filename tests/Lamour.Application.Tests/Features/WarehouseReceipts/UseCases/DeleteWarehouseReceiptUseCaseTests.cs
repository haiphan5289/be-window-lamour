using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Application.Features.WarehouseReceipts.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.WarehouseReceipts.UseCases;

// 2026-10-09: form "Phiếu nhập kho" có nút Xóa (chỉ bật với phiếu nháp) — BE vẫn là chốt chặn cuối:
// phiếu đã ghi sổ đã cộng tồn kho nên phải "Bỏ ghi" trước khi xóa.
public class DeleteWarehouseReceiptUseCaseTests
{
    private readonly Mock<IWarehouseReceiptRepository> _repo = new();

    private DeleteWarehouseReceiptUseCase CreateSut() =>
        new(_repo.Object, Mock.Of<ILogger<DeleteWarehouseReceiptUseCase>>());

    [Fact]
    public async Task DraftReceipt_IsDeleted()
    {
        var receipt = new WarehouseReceipt { Id = 7, ReceiptNumber = "NK00007", Status = WarehouseReceiptStatus.Draft };
        _repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        await CreateSut().ExecuteAsync(7);

        _repo.Verify(r => r.DeleteAsync(receipt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmedReceipt_ThrowsDomainException_AndIsNotDeleted()
    {
        var receipt = new WarehouseReceipt { Id = 8, ReceiptNumber = "NK00008", Status = WarehouseReceiptStatus.Confirmed };
        _repo.Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        await Assert.ThrowsAsync<DomainException>(() => CreateSut().ExecuteAsync(8));

        _repo.Verify(r => r.DeleteAsync(It.IsAny<WarehouseReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnknownReceipt_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseReceipt?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().ExecuteAsync(99));
    }
}
