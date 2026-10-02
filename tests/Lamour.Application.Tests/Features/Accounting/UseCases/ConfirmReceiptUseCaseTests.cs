using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

public class ConfirmReceiptUseCaseTests
{
    private static Receipt MakeReceipt(PaymentPartnerType? partnerType, ReceiptStatus status = ReceiptStatus.Draft) => new()
    {
        Id = 1, DocumentNumber = "PT00106", PayerName = "TRẦN THỊ NHI TRÚC", Status = status,
        PartnerType = partnerType, PartnerId = partnerType is null ? null : 16,
        AccountingDate = DateTime.UtcNow, DocumentDate = DateTime.UtcNow,
        Entries =
        {
            new ReceiptEntry
            {
                Amount = 50_000_000m,
                DebitAccountSetting  = new AccountSetting { Code = "1111" },
                CreditAccountSetting = new AccountSetting { Code = "1388" },
            },
        },
    };

    private static (ConfirmReceiptUseCase Sut, Mock<ICashLedgerRepository> Cash) CreateSut(Receipt receipt)
    {
        var repo = new Mock<IReceiptRepository>();
        repo.Setup(r => r.GetByIdTrackedAsync(receipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        var cash = new Mock<ICashLedgerRepository>();
        cash.Setup(c => c.AddAsync(It.IsAny<CashTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CashTransaction t, CancellationToken _) => t);
        return (new ConfirmReceiptUseCase(repo.Object, cash.Object, Mock.Of<ILogger<ConfirmReceiptUseCase>>()), cash);
    }

    [Theory]
    [InlineData(PaymentPartnerType.Employee, "Phiếu thu")]
    [InlineData(PaymentPartnerType.Customer, "Phiếu thu tiền mặt khách hàng")]
    [InlineData(null,                        "Phiếu thu tiền mặt khách hàng hàng loạt")]
    public async Task Confirm_PostsCash_WithCatalogAccounts_AndDocumentTypeByPartner(
        PaymentPartnerType? partnerType, string expectedDocumentType)
    {
        var receipt = MakeReceipt(partnerType);
        var (sut, cash) = CreateSut(receipt);

        var result = await sut.ExecuteAsync(receipt.Id);

        Assert.Equal("Confirmed", result.Status);
        cash.Verify(c => c.AddAsync(It.Is<CashTransaction>(t =>
                t.DebitAmount == 50_000_000m && t.ReceiptNumber == "PT00106"
                && t.Account == "111" && t.CounterAccount == "1388"
                && t.DocumentType == expectedDocumentType),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlreadyConfirmed_IsRejected()
    {
        var (sut, cash) = CreateSut(MakeReceipt(PaymentPartnerType.Employee, ReceiptStatus.Confirmed));

        await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(1));
        cash.Verify(c => c.AddAsync(It.IsAny<CashTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
