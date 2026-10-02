using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

// Báo cáo "Sổ kế toán chi tiết quỹ tiền mặt": dòng sổ quỹ có phiếu gốc thì bung ra từng dòng hạch toán,
// dòng cũ (nhập từ MISA, không còn phiếu gốc) giữ nguyên; số tồn đầu kỳ tính từ sổ quỹ.
public class GetCashLedgerDetailReportUseCaseTests
{
    private const decimal Initial = 1_000m;
    private static readonly DateTime Dec31 = new(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan05 = new(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan07 = new(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan31 = new(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

    private static AccountSetting Acc(string code) => new() { Code = code };

    private static Receipt BulkReceipt() => new()
    {
        Id = 18, DocumentNumber = "PT00018", PayerName = "thanh đức", Status = ReceiptStatus.Confirmed,
        PaymentReason = PaymentReason.ThuKhachHangHangLoat, CreatedAt = Jan05.AddHours(9),
        Entries =
        {
            new ReceiptEntry { Id = 1, Description = "Thu tiền khách hàng", Amount = 400m, DebitAccountSetting = Acc("1111"), CreditAccountSetting = Acc("131") },
            new ReceiptEntry { Id = 2, Description = "Thu tiền khách hàng", Amount = 100m, DebitAccountSetting = Acc("1111"), CreditAccountSetting = Acc("131") },
        },
    };

    private static Payment Payment84() => new()
    {
        Id = 84, DocumentNumber = "PC00084", PayeeName = "LÊ HOÀNG THANH ĐỨC", Status = PaymentStatus.Confirmed,
        CreatedAt = Jan05.AddHours(8),
        Entries =
        {
            new PaymentEntry { Id = 1, Description = "hoàn trả HDD số 128", Amount = 163m, DebitAccountSetting = Acc("6418"), CreditAccountSetting = Acc("1111"),
                               ExpenseCategory = new ExpenseCategory { Code = "01", Name = "PHÒNG KINH DOANH" } },
        },
    };

    private static GetCashLedgerDetailReportUseCase CreateSut(
        List<CashTransaction> transactions, List<Receipt>? receipts = null, List<Payment>? payments = null)
    {
        var cash = new Mock<ICashLedgerRepository>();
        cash.SetupGet(r => r.InitialBalance).Returns(Initial);
        cash.Setup(r => r.GetUpToDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(transactions);
        var receiptRepo = new Mock<IReceiptRepository>();
        receiptRepo.Setup(r => r.GetByDocumentNumbersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((receipts ?? new()).ToDictionary(r => r.DocumentNumber));
        var paymentRepo = new Mock<IPaymentRepository>();
        paymentRepo.Setup(r => r.GetByDocumentNumbersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((payments ?? new()).ToDictionary(p => p.DocumentNumber));
        return new GetCashLedgerDetailReportUseCase(cash.Object, receiptRepo.Object, paymentRepo.Object,
            Mock.Of<ILogger<GetCashLedgerDetailReportUseCase>>());
    }

    private static List<CashTransaction> Ledger() => new()
    {
        // Dòng cũ nhập từ MISA, trước kỳ → vào số tồn đầu kỳ.
        new() { Id = 1, AccountingDate = Dec31, DocumentDate = Dec31, ReceiptNumber = "PT00678", Account = "111", CounterAccount = "131", DebitAmount = 200m, Description = "Thu tiền khách hàng", PersonName = "cũ", CreatedAt = Dec31 },
        // Phiếu do app ghi sổ: 1 dòng tổng / phiếu.
        new() { Id = 2, AccountingDate = Jan05, DocumentDate = Jan05, ReceiptNumber = "PT00018", Account = "111", CounterAccount = "131", DebitAmount = 500m, CreatedAt = Jan05.AddHours(9) },
        new() { Id = 3, AccountingDate = Jan05, DocumentDate = Jan05, PaymentNumber = "PC00084", Account = "111", CounterAccount = "6418", CreditAmount = 163m, CreatedAt = Jan05.AddHours(8) },
        // Dòng cũ trong kỳ, không còn phiếu gốc.
        new() { Id = 4, AccountingDate = Jan07, DocumentDate = Jan07, PaymentNumber = "PC02216", Account = "111", CounterAccount = "6418", CreditAmount = 50m, Description = "đổ xăng", PersonName = "NV A", CreatedAt = Jan07 },
    };

    [Fact]
    public async Task ExpandsVoucherLines_KeepsLegacyRows_AndRunsBalanceFromOpening()
    {
        var sut = CreateSut(Ledger(), new() { BulkReceipt() }, new() { Payment84() });

        var result = await sut.ExecuteAsync(Jan05, Jan31, Array.Empty<string>(), false, false);

        Assert.Equal(1_200m, result.OpeningBalance);            // số dư gốc 1.000 + 200 trước kỳ
        Assert.Equal(4, result.Rows.Count);                     // PC00084 (1) + PT00018 (2 dòng) + PC02216 (1)

        var pc = result.Rows[0];                                // cùng ngày: phiếu lập trước đứng trước
        Assert.Equal("PC00084", pc.PaymentNumber);
        Assert.Equal("1111", pc.Account);
        Assert.Equal("6418", pc.CounterAccount);
        Assert.Equal(163m, pc.CreditAmount);
        Assert.Equal("01", pc.CategoryCode);
        Assert.Equal(84, pc.PaymentId);
        Assert.Equal(1_037m, pc.Balance);

        Assert.Equal(new[] { 400m, 100m }, result.Rows.Skip(1).Take(2).Select(r => r.DebitAmount));
        Assert.All(result.Rows.Skip(1).Take(2), r => { Assert.Equal(18, r.ReceiptId); Assert.True(r.IsBulkReceipt); Assert.Equal("thanh đức", r.PersonName); });

        var legacy = result.Rows[3];
        Assert.Equal("đổ xăng", legacy.Description);
        Assert.Null(legacy.PaymentId);                          // không còn phiếu gốc → không bấm được

        Assert.Equal(500m, result.TotalDebit);
        Assert.Equal(213m, result.TotalCredit);
        Assert.Equal(1_487m, result.ClosingBalance);
        Assert.Equal(result.ClosingBalance, result.Rows[^1].Balance);
    }

    [Fact]
    public async Task MergeSimilar_SumsLinesOfSameVoucherAndDescription()
    {
        var sut = CreateSut(Ledger(), new() { BulkReceipt() }, new() { Payment84() });

        var result = await sut.ExecuteAsync(Jan05, Jan31, Array.Empty<string>(), mergeSimilar: true, orderByCreated: false);

        var pt = Assert.Single(result.Rows, r => r.ReceiptNumber == "PT00018");
        Assert.Equal(500m, pt.DebitAmount);
        Assert.Equal(1_487m, result.ClosingBalance);
    }

    [Fact]
    public async Task OrderByCreated_SortsByVoucherCreationTime()
    {
        var late = Payment84();
        late.CreatedAt = Jan31;                                 // lập sau cùng dù ngày hạch toán 05/01
        var sut = CreateSut(Ledger(), new() { BulkReceipt() }, new() { late });

        var result = await sut.ExecuteAsync(Jan05, Jan31, Array.Empty<string>(), false, orderByCreated: true);

        Assert.Equal("PC00084", result.Rows[^1].PaymentNumber);
    }

    [Fact]
    public async Task AccountFilter_ChildOnly_ExcludesParentPostedRows_AndInitialBalance()
    {
        var sut = CreateSut(Ledger(), new() { BulkReceipt() }, new() { Payment84() });

        var result = await sut.ExecuteAsync(Jan05, Jan31, new[] { "1111" }, false, false);

        Assert.Equal(0m, result.OpeningBalance);                // số dư gốc + dòng cũ đều thuộc TK 111
        Assert.Equal(3, result.Rows.Count);
        Assert.All(result.Rows, r => Assert.Equal("1111", r.Account));
    }

    [Fact]
    public async Task BankReceipt_IsNotPartOfCashFund()
    {
        var bank = new Receipt
        {
            Id = 30, DocumentNumber = "PT00030", PayerName = "KH", Status = ReceiptStatus.Confirmed,
            PartnerType = PaymentPartnerType.Customer, PartnerId = 1,
            Entries = { new ReceiptEntry { Id = 1, Description = "CK", Amount = 900m, DebitAccountSetting = Acc("1121"), CreditAccountSetting = Acc("131") } },
        };
        var ledger = new List<CashTransaction>
        {
            new() { Id = 9, AccountingDate = Jan05, DocumentDate = Jan05, ReceiptNumber = "PT00030", Account = "112", CounterAccount = "131", DebitAmount = 900m },
        };
        var sut = CreateSut(ledger, new() { bank });

        var result = await sut.ExecuteAsync(Jan05, Jan31, Array.Empty<string>(), false, false);

        Assert.Empty(result.Rows);
        Assert.Equal(Initial, result.ClosingBalance);
    }
}
