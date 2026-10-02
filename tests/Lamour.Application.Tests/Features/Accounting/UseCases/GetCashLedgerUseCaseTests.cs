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
        Dictionary<string, int>? paymentIds = null,
        HashSet<int>? bulkReceiptIds = null,
        Dictionary<int, string>? reasonDetails = null,
        Dictionary<int, string>? receiptReasonDetails = null)
    {
        _receipts.Setup(r => r.GetReasonDetailsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(receiptReasonDetails ?? new Dictionary<int, string>());
        _payments.Setup(r => r.GetReasonDetailsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(reasonDetails ?? new Dictionary<int, string>());
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
        _receipts.Setup(r => r.GetBulkReceiptIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(bulkReceiptIds ?? new HashSet<int>());

        return new GetCashLedgerUseCase(_cash.Object, _payments.Object, _receipts.Object,
            Mock.Of<ILogger<GetCashLedgerUseCase>>());
    }

    [Fact]
    public async Task DraftReceipt_IsListed_WithId_ButDoesNotMoveBalance()
    {
        var draft = new Receipt
        {
            Id = 7, DocumentNumber = "PT00007", PayerName = "Chị Lan", CustomerId = 1,
            PartnerType = PaymentPartnerType.Customer, PartnerId = 1,
            AccountingDate = Day, DocumentDate = Day, Status = ReceiptStatus.Draft,
            Entries = { new ReceiptEntry { Amount = 500m, DebitAccountSetting = new AccountSetting { Code = "1111" }, CreditAccountSetting = new AccountSetting { Code = "131" } } },
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

    [Fact]
    public async Task BulkReceipts_AreFlagged_BothPostedAndTreo()
    {
        var treoBulk = new Receipt
        {
            Id = 9, DocumentNumber = "PT00009", PayerName = "Thu tiền khách hàng hàng loạt", CustomerId = null,
            AccountingDate = Day, DocumentDate = Day, Status = ReceiptStatus.Draft,
            Entries = { new ReceiptEntry { Amount = 800m, DebitAccountSetting = new AccountSetting { Code = "1111" }, CreditAccountSetting = new AccountSetting { Code = "131" } } },
        };
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00001", DebitAmount = 300m }, // phiếu thường
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00004", DebitAmount = 400m }, // hàng loạt
            },
            unconfirmedReceipts: new() { treoBulk },
            receiptIds: new() { ["PT00001"] = 11, ["PT00004"] = 14 },
            bulkReceiptIds: new() { 14 });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.False(result.Entries.Single(e => e.ReceiptNumber == "PT00001").IsBulkReceipt);
        Assert.True(result.Entries.Single(e => e.ReceiptNumber == "PT00004").IsBulkReceipt);
        Assert.True(result.Entries.Single(e => e.ReceiptNumber == "PT00009").IsBulkReceipt);
    }

    [Fact]
    public async Task Description_Receipt_UsesReasonLabel_NotPayerName()
    {
        var receipt = new Receipt
        {
            Id = 3, DocumentNumber = "PT00003", PayerName = "Nhi Trúc", CustomerId = null,
            PaymentReason = PaymentReason.ThuKhachHangHangLoat,
            AccountingDate = Day, DocumentDate = Day, Status = ReceiptStatus.Draft,
            Entries = { new ReceiptEntry { Amount = 100m, DebitAccountSetting = new AccountSetting { Code = "1111" }, CreditAccountSetting = new AccountSetting { Code = "131" } } },
        };
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00004", DebitAmount = 400m,
                    Description = "Nhi Trúc", PersonName = "Nhi Trúc", PaymentReason = "ThuKhachHangHangLoat" },
            },
            unconfirmedReceipts: new() { receipt },
            receiptIds: new() { ["PT00004"] = 14 },
            bulkReceiptIds: new() { 14 });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.All(result.Entries, e => Assert.Equal("Thu tiền khách hàng", e.Description));
        Assert.All(result.Entries, e => Assert.Equal("Nhi Trúc", e.PersonName)); // tên thật vẫn hiện
    }

    [Fact]
    public async Task PersonName_BulkReceiptWithDefaultPayerName_IsBlank()
    {
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00004", DebitAmount = 400m,
                    PersonName = CreateBulkCustomerReceiptUseCase.DefaultPayerName },
                // Phiếu thu thường trùng tên mặc định → giữ nguyên (chỉ phiếu hàng loạt bị để trống).
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00001", DebitAmount = 100m,
                    PersonName = CreateBulkCustomerReceiptUseCase.DefaultPayerName },
            },
            receiptIds: new() { ["PT00004"] = 14, ["PT00001"] = 11 },
            bulkReceiptIds: new() { 14 });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.Null(result.Entries.Single(e => e.ReceiptNumber == "PT00004").PersonName);
        Assert.NotNull(result.Entries.Single(e => e.ReceiptNumber == "PT00001").PersonName);
    }

    [Fact]
    public async Task Description_Payment_UsesReasonDetail_ElseReasonLabel()
    {
        var treo = new Payment
        {
            Id = 5, DocumentNumber = "PC00005", PayeeName = "NCC A", PaymentReason = PaymentReason.ChiMuaHang,
            AccountingDate = Day, DocumentDate = Day, Status = PaymentStatus.Draft,
            Entries = { new PaymentEntry { Amount = 200m, DebitAccountSetting = new AccountSetting { Code = "331" } } },
        };
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, PaymentNumber = "PC00002", CreditAmount = 100m,
                    Description = "LÊ HOÀNG THANH ĐỨC", PersonName = "LÊ HOÀNG THANH ĐỨC", PaymentReason = "ChiKhac" },
                new CashTransaction { AccountingDate = Day, PaymentNumber = "PC00003", CreditAmount = 50m,
                    PersonName = "NGUYỄN MINH TRUNG", PaymentReason = "ChiKhac" },
            },
            unconfirmedPayments: new() { treo },
            paymentIds: new() { ["PC00002"] = 22, ["PC00003"] = 23 },
            reasonDetails: new() { [22] = "cước đt + gia hạn zalo" });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.Equal("cước đt + gia hạn zalo", result.Entries.Single(e => e.PaymentNumber == "PC00002").Description);
        Assert.Equal("Chi khác", result.Entries.Single(e => e.PaymentNumber == "PC00003").Description);
        Assert.Equal("Chi mua hàng", result.Entries.Single(e => e.PaymentNumber == "PC00005").Description);
        Assert.Equal("LÊ HOÀNG THANH ĐỨC", result.Entries.Single(e => e.PaymentNumber == "PC00002").PersonName);
    }

    [Theory]
    [InlineData("TamUngNhanVien",  "Tạm ứng cho nhân viên")]
    [InlineData("GuiTienNganHang", "Gửi tiền vào ngân hàng")]
    [InlineData("ThueTNDNTamTinh", "Thuế TNDN tạm tính")]
    [InlineData("ChiKhac",         "Chi khác")]
    public async Task Description_Payment_NewReasons_HaveVietnameseLabel(string reason, string expected)
    {
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, PaymentNumber = "PC00007", CreditAmount = 10m, PaymentReason = reason },
            },
            paymentIds: new() { ["PC00007"] = 7 });

        var result = await sut.ExecuteAsync(Day, Day);

        Assert.Equal(expected, Assert.Single(result.Entries).Description);
    }

    // 2026-10-01: phiếu thu của Nhân viên có CustomerId == null nhưng KHÔNG phải phiếu hàng loạt; Diễn giải
    // lấy "Lý do nộp" chi tiết, TK đối ứng là mã TK Có thật trong danh mục (vd 1388).
    [Fact]
    public async Task EmployeeReceipt_IsNotBulk_AndUsesReasonDetail_BothPostedAndTreo()
    {
        var treo = new Receipt
        {
            Id = 21, DocumentNumber = "PT00106", PayerName = "TRẦN THỊ NHI TRÚC", CustomerId = null,
            PartnerType = PaymentPartnerType.Employee, PartnerId = 16, PartnerName = "TRẦN THỊ NHI TRÚC",
            PaymentReason = PaymentReason.ThuKhac, ReasonDetail = "nhập quỹ",
            AccountingDate = Day, DocumentDate = Day, Status = ReceiptStatus.Draft,
            Entries = { new ReceiptEntry { Amount = 50_000_000m, DebitAccountSetting = new AccountSetting { Code = "1111" }, CreditAccountSetting = new AccountSetting { Code = "1388" } } },
        };
        var sut = CreateSut(
            transactions: new()
            {
                new CashTransaction { AccountingDate = Day, ReceiptNumber = "PT00100", DebitAmount = 50_000_000m,
                    PersonName = "LÊ HOÀNG THANH ĐỨC", PaymentReason = "ThuKhac", DocumentType = "Phiếu thu" },
            },
            unconfirmedReceipts: new() { treo },
            receiptIds: new() { ["PT00100"] = 20 },
            receiptReasonDetails: new() { [20] = "nhập quỹ ( a khoa 4)" });

        var result = await sut.ExecuteAsync(Day, Day);

        var posted = result.Entries.Single(e => e.ReceiptNumber == "PT00100");
        Assert.Equal("nhập quỹ ( a khoa 4)", posted.Description);
        Assert.False(posted.IsBulkReceipt);

        var row = result.Entries.Single(e => e.ReceiptNumber == "PT00106");
        Assert.Equal("nhập quỹ", row.Description);
        Assert.Equal("Phiếu thu", row.DocumentType);
        Assert.Equal("111", row.Account);
        Assert.Equal("1388", row.CounterAccount);
        Assert.False(row.IsBulkReceipt);
        Assert.Equal("TRẦN THỊ NHI TRÚC", row.PersonName);
    }
}
