using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetCashLedgerUseCase : IGetCashLedgerUseCase
{
    private readonly ICashLedgerRepository _repo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IReceiptRepository _receiptRepo;
    private readonly ILogger<GetCashLedgerUseCase> _logger;

    public GetCashLedgerUseCase(
        ICashLedgerRepository repo,
        IPaymentRepository paymentRepo,
        IReceiptRepository receiptRepo,
        ILogger<GetCashLedgerUseCase> logger)
    {
        _repo        = repo;
        _paymentRepo = paymentRepo;
        _receiptRepo = receiptRepo;
        _logger      = logger;
    }

    public async Task<CashLedgerResponseDto> ExecuteAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching cash ledger from {From} to {To}", from, to);

        var openingBalance      = await _repo.GetBalanceBeforeDateAsync(from, ct);
        // Tồn quỹ đến hiện tại — độc lập với filter from/to, luôn tính tới hết HÔM NAY (UTC).
        var currentBalance      = await _repo.GetBalanceBeforeDateAsync(DateTime.UtcNow.Date.AddDays(1), ct);
        var transactions        = await _repo.GetByDateRangeAsync(from, to, ct);
        var unconfirmedPayments = await _paymentRepo.GetUnconfirmedByDateRangeAsync(from, to, ct);
        var unconfirmedReceipts = await _receiptRepo.GetUnconfirmedByDateRangeAsync(from, to, ct);

        // CashTransaction chỉ lưu số chứng từ — tra lại id phiếu gốc để màn Quỹ Ghi sổ/Bỏ ghi/Xóa
        // được trên dòng đã ghi sổ.
        var receiptIds = await _receiptRepo.GetIdsByDocumentNumbersAsync(
            transactions.Where(t => t.ReceiptNumber != null).Select(t => t.ReceiptNumber!), ct);
        var paymentIds = await _paymentRepo.GetIdsByDocumentNumbersAsync(
            transactions.Where(t => t.PaymentNumber != null).Select(t => t.PaymentNumber!), ct);
        // Dòng đã ghi sổ chỉ có số phiếu — tra thêm phiếu nào là hàng loạt (dòng Treo đã có sẵn CustomerId).
        var bulkReceiptIds = await _receiptRepo.GetBulkReceiptIdsAsync(receiptIds.Values, ct);
        // Diễn giải phiếu chi = "Lý do chi" chi tiết của phiếu gốc (CashTransaction.Description chỉ lưu tên người nhận).
        var reasonDetails = await _paymentRepo.GetReasonDetailsByIdsAsync(
            paymentIds.Values.Concat(unconfirmedPayments.Select(p => p.Id)), ct);

        // Confirmed rows come from posted CashTransactions; Draft/Treo rows are Payments (và từ
        // 2026-09-26 cả Receipts) not yet ghi số — shown for visibility only, they must not move
        // the running balance.
        var rows = transactions
            .Select(t => new CashLedgerEntryDto
            {
                AccountingDate = t.AccountingDate,
                DocumentDate   = t.DocumentDate,
                ReceiptNumber  = t.ReceiptNumber,
                PaymentNumber  = t.PaymentNumber,
                Description    = t.PaymentNumber != null
                                 && paymentIds.TryGetValue(t.PaymentNumber, out var descPid)
                                 && reasonDetails.TryGetValue(descPid, out var detail)
                    ? detail
                    : ReasonLabel(t.PaymentReason),
                Account        = t.Account,
                CounterAccount = t.CounterAccount,
                DebitAmount    = t.DebitAmount,
                CreditAmount   = t.CreditAmount,
                Amount         = t.DebitAmount != 0m ? t.DebitAmount : t.CreditAmount,
                PersonName     = SubjectName(t.PersonName,
                    t.ReceiptNumber != null && receiptIds.TryGetValue(t.ReceiptNumber, out var pnId) && bulkReceiptIds.Contains(pnId)),
                PaymentReason  = t.PaymentReason,
                DocumentType   = t.DocumentType,
                Status         = "Confirmed",
                PostedAt       = t.CreatedAt,
                ReceiptId      = t.ReceiptNumber != null && receiptIds.TryGetValue(t.ReceiptNumber, out var rid) ? rid : null,
                PaymentId      = t.PaymentNumber != null && paymentIds.TryGetValue(t.PaymentNumber, out var pid) ? pid : null,
                IsBulkReceipt  = t.ReceiptNumber != null && receiptIds.TryGetValue(t.ReceiptNumber, out var bid) && bulkReceiptIds.Contains(bid),
            })
            .Concat(unconfirmedPayments
                .Where(p => p.Entries.Count > 0)
                .Select(p =>
                {
                    var totalAmount = p.Entries.Sum(e => e.Amount);
                    return new CashLedgerEntryDto
                    {
                        AccountingDate = p.AccountingDate,
                        DocumentDate   = p.DocumentDate,
                        ReceiptNumber  = null,
                        PaymentNumber  = p.DocumentNumber,
                        Description    = reasonDetails.TryGetValue(p.Id, out var pd) ? pd : ReasonLabel(p.PaymentReason.ToString()),
                        Account        = "111",
                        CounterAccount = p.Entries.First().DebitAccountSetting.Code,
                        DebitAmount    = 0m,
                        CreditAmount   = totalAmount,
                        Amount         = totalAmount,
                        PersonName     = p.PayeeName,
                        PaymentReason  = p.PaymentReason.ToString(),
                        DocumentType   = "Phiếu chi",
                        Status         = "Treo", // không còn "Nháp" (khớp Chứng từ bán hàng): Draft/Treo đều là "chưa ghi sổ"
                        PaymentId      = p.Id,
                    };
                }))
            // Phiếu thu chưa ghi sổ — cùng field-mapping với ConfirmReceiptUseCase (lúc ghi sổ sẽ
            // tạo CashTransaction đúng các giá trị này), để dòng không "nhảy" nội dung sau khi ghi sổ.
            .Concat(unconfirmedReceipts
                .Where(r => r.Entries.Count > 0)
                .Select(r =>
                {
                    var totalAmount = r.Entries.Sum(e => e.Amount);
                    var first       = r.Entries.First();
                    return new CashLedgerEntryDto
                    {
                        AccountingDate = r.AccountingDate,
                        DocumentDate   = r.DocumentDate,
                        ReceiptNumber  = r.DocumentNumber,
                        PaymentNumber  = null,
                        Description    = ReasonLabel(r.PaymentReason.ToString()),
                        Account        = CreateReceiptUseCase.MapAccountCodeToString(first.DebitAccount),
                        CounterAccount = CreateReceiptUseCase.MapAccountCodeToString(first.CreditAccount),
                        DebitAmount    = totalAmount,
                        CreditAmount   = 0m,
                        Amount         = totalAmount,
                        PersonName     = SubjectName(r.PayerName, r.CustomerId is null),
                        PaymentReason  = r.PaymentReason.ToString(),
                        DocumentType   = r.CustomerId is null
                            ? "Phiếu thu tiền mặt khách hàng hàng loạt"
                            : "Phiếu thu tiền mặt khách hàng",
                        Status         = "Treo",
                        ReceiptId      = r.Id,
                        IsBulkReceipt  = r.CustomerId is null,
                    };
                }))
            .OrderBy(e => e.AccountingDate)
            .ToList();

        var runningBalance = openingBalance;
        foreach (var row in rows)
        {
            if (row.Status == "Confirmed")
                runningBalance += row.DebitAmount - row.CreditAmount;
            row.Balance = runningBalance;
        }

        return new CashLedgerResponseDto
        {
            OpeningBalance = openingBalance,
            ClosingBalance = rows.Count > 0 ? rows[^1].Balance : openingBalance,
            CurrentBalance = currentBalance,
            Entries        = rows,
        };
    }

    // Diễn giải khi phiếu không có "Lý do chi" chi tiết = nhãn lý do (cùng chữ ở ô Lý do nộp/Lý do chi
    // trên popup phiếu). Giá trị lạ (dữ liệu cũ) giữ nguyên chuỗi gốc.
    private static string ReasonLabel(string? paymentReason) => paymentReason switch
    {
        "ThuKhac"              => "Thu khác",
        "ThuTienHang"          => "Thu tiền hàng",
        "ThuCongNo"            => "Thu công nợ",
        "ThuKhachHangHangLoat" => "Thu tiền khách hàng",
        "ChiKhac"              => "Chi khác",
        "ChiMuaHang"           => "Chi mua hàng",
        "ChiTraNo"             => "Chi trả nợ",
        "ChiLuong"             => "Chi lương",
        "TamUngNhanVien"       => "Tạm ứng cho nhân viên",
        "GuiTienNganHang"      => "Gửi tiền vào ngân hàng",
        "ThueTNDNTamTinh"      => "Thuế TNDN tạm tính",
        _                      => paymentReason ?? "",
    };

    // Cột "Đối tượng": phiếu thu hàng loạt không nhập người nộp thì để trống (khớp MISA) thay vì hiện
    // tên mặc định do BE tự điền.
    private static string? SubjectName(string? name, bool isBulkReceipt) =>
        isBulkReceipt && name == CreateBulkCustomerReceiptUseCase.DefaultPayerName ? null : name;
}
