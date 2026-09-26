using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

public class GetCashLedgerUseCaseTests
{
    private static readonly DateTime Day = new(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ICashLedgerRepository> _cash     = new();
    private readonly Mock<IPaymentRepository>    _payments = new();
    private readonly Mock<IReceiptRepository>    _receipts = new();

    private GetCashLedgerUseCase CreateSut(
        List<CashTransaction>? transactions = null,
        List<Payment>? unconfirmedPayments = null,
        List<Receipt>? unconfirmedReceipts = null,
        Dictionary<string, int>? receiptIds = null,
        Dictionary<string, int>? paymentIds = null)
    {
        _cash.Setup(r => r.GetBalanceBeforeDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(1_000m);
        _cash.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(transactions ?? new List<CashTransaction>());
        _payments.Setup(r => r.GetUnconfirmedByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(unconfirmedPayments ?? new List<Payment>());
        _receipts.Setup(r => r.GetUnconfirmedByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(unconfirmedReceipts ?? new List<Receipt>());
        _receipts.Setup(r => r.GetIdsByDocumentNumbersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(receiptIds ?? new Dictionary<string, int>());
        _payments.Setup(r => r.GetIdsByDocumentNumbersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paymentIds ?? new Dictionary<string, int>());

        return new GetCashLedgerUseCase(_cash.Object, _payments.Object, _receipts.Object,
            Mock.Of<ILogger<GetCashLedgerUseCase>>());
    }

    [Fact]
    public async Task DraftReceipt_IsListed_WithId_ButDoesNotMoveBalance()
    {
        var draft = new Receipt
        {
            Id = 7, DocumentNumber = "PT00007", PayerName = "Chị Lan", CustomerId = 1,
            AccountingDate = Day, DocumentDate = Day, Status = ReceiptStatus.Draft,
            Entries = { new ReceiptEntry { Amount = 500m, DebitAccount = AccountCode.Cash111, CreditAccount = AccountCode.Receivable131 } },
        };
        var sut = CreateSut(unconfirmedReceipts: new() { draft });

        var result = await sut.ExecuteAsync(Day, Day);

        var row = Assert.Single(result.Entries);
        Assert.Equal("PT00007", row.ReceiptNumber);
        Assert.Equal(7, row.ReceiptId);
        Assert.Null(row.PaymentId);
        Assert.Equal("Treo", row.Status);         // không còn "Nháp" — chưa ghi sổ = Treo
        Assert.Equal(500m, row.Amount);
        Assert.Equal(1_000m, row.Balance);        // Nháp không làm đổi số tồn
        Assert.Equal(1_000m, result.ClosingBalance);
    }

    [Fact]
    public async Task ConfirmedRows_GetIdsResolvedFromDocumentNumbers()
    {
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00001", DebitAmount = 300m },
                new CashTransaction { AccountingDate = Day, PaymentNumber = "PC00002", CreditAmount = 100m },
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT09999", DebitAmount = 50m }, // phiếu gốc đã xoá
            },
            receiptIds: new() { ["PT00001"] = 11 },
            paymentIds: new() { ["PC00002"] = 22 });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.Equal(11, result.Entries.Single(e => e.ReceiptNumber == "PT00001").ReceiptId);
        Assert.Equal(22, result.Entries.Single(e => e.PaymentNumber == "PC00002").PaymentId);
        Assert.Null(result.Entries.Single(e => e.ReceiptNumber == "PT09999").ReceiptId);
        Assert.Equal(1_000m + 300m - 100m + 50m, result.ClosingBalance);
    }

    [Fact]
    public async Task UnconfirmedPayment_CarriesItsId()
    {
        var payment = new Payment
        {
            Id = 5, DocumentNumber = "PC00005", PayeeName = "NCC A",
            AccountingDate = Day, DocumentDate = Day, Status = PaymentStatus.Draft,
            Entries = { new PaymentEntry { Amount = 200m, DebitAccountSetting = new AccountSetting { Code = "331" } } },
        };
        var sut = CreateSut(unconfirmedPayments: new() { payment });

        var result = await sut.ExecuteAsync(Day, Day);

        var row = Assert.Single(result.Entries);
        Assert.Equal(5, row.PaymentId);
        Assert.Equal(1_000m, row.Balance);
    }
}
