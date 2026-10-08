using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Application.Features.WarehouseReceipts.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.WarehouseReceipts.UseCases;

// 2026-10-08: số phiếu nhập kho chỉ để xem trước trên form "Phiếu nhập kho" — số thật vẫn sinh lúc tạo phiếu.
public class GetNextWarehouseReceiptNumberUseCaseTests
{
    private readonly Mock<IWarehouseReceiptRepository> _repo = new();

    private GetNextWarehouseReceiptNumberUseCase CreateSut() =>
        new(_repo.Object, Mock.Of<ILogger<GetNextWarehouseReceiptNumberUseCase>>());

    [Fact]
    public async Task ReturnsNumberFromRepository()
    {
        _repo.Setup(r => r.GetNextReceiptNumberAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync("NK00055");

        var result = await CreateSut().ExecuteAsync();

        Assert.Equal("NK00055", result);
        _repo.Verify(r => r.GetNextReceiptNumberAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PassesCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        _repo.Setup(r => r.GetNextReceiptNumberAsync(cts.Token)).ReturnsAsync("NK00001");

        var result = await CreateSut().ExecuteAsync(cts.Token);

        Assert.Equal("NK00001", result);
        _repo.Verify(r => r.GetNextReceiptNumberAsync(cts.Token), Times.Once);
    }
}
