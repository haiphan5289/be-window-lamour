using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

// 2026-09-26: quy trình Quỹ bỏ "Nháp" (khớp Chứng từ bán hàng) — phiếu chi chưa ghi sổ (Draft cũ
// hoặc Treo) đều ghi sổ thẳng được; chỉ chặn phiếu đã ghi sổ.
public class ConfirmPaymentUseCaseTests
{
    private static Payment MakePayment(PaymentStatus status) => new()
    {
        Id = 1, DocumentNumber = "PC00001", PayeeName = "NCC A", Status = status,
        AccountingDate = DateTime.UtcNow, DocumentDate = DateTime.UtcNow,
        Entries = { new PaymentEntry { Amount = 250m, DebitAccountSetting = new AccountSetting { Code = "331" } } },
    };

    private static (ConfirmPaymentUseCase Sut, Mock<ICashLedgerRepository> Cash) CreateSut(Payment payment)
    {
        var repo = new Mock<IPaymentRepository>();
        repo.Setup(r => r.GetByIdTrackedAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        var cash = new Mock<ICashLedgerRepository>();
        cash.Setup(c => c.AddAsync(It.IsAny<CashTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CashTransaction t, CancellationToken _) => t);
        return (new ConfirmPaymentUseCase(repo.Object, cash.Object, Mock.Of<ILogger<ConfirmPaymentUseCase>>()), cash);
    }

    [Theory]
    [InlineData(PaymentStatus.Draft)]
    [InlineData(PaymentStatus.Treo)]
    public async Task UnpostedPayment_IsConfirmed_AndPostsCash(PaymentStatus status)
    {
        var payment = MakePayment(status);
        var (sut, cash) = CreateSut(payment);

        var result = await sut.ExecuteAsync(payment.Id);

        Assert.Equal("Confirmed", result.Status);
        cash.Verify(c => c.AddAsync(It.Is<CashTransaction>(t => t.CreditAmount == 250m && t.PaymentNumber == "PC00001"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlreadyConfirmed_IsRejected()
    {
        var (sut, cash) = CreateSut(MakePayment(PaymentStatus.Confirmed));

        await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(1));
        cash.Verify(c => c.AddAsync(It.IsAny<CashTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
