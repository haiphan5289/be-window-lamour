using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// Báo cáo "Sổ kế toán chi tiết quỹ tiền mặt" (khớp mẫu MISA): mỗi dòng là 1 DÒNG HẠCH TOÁN, có Số tồn
// cộng dồn và dòng Số tồn đầu kỳ.
//
// Nguồn = sổ quỹ (CashTransaction), để số tồn khớp màn Quỹ. Sổ quỹ có 2 loại dòng:
//   • dòng cũ nhập từ MISA — mỗi dòng hạch toán đã là 1 dòng sổ, không còn phiếu gốc → dùng nguyên;
//   • dòng do app ghi sổ — 1 dòng TỔNG cho mỗi phiếu → bung ra từng dòng hạch toán của phiếu gốc.
public class GetCashLedgerDetailReportUseCase : IGetCashLedgerDetailReportUseCase
{
    private const string CashAccountPrefix = "111";

    private readonly ICashLedgerRepository _repo;
    private readonly IReceiptRepository    _receiptRepo;
    private readonly IPaymentRepository    _paymentRepo;
    private readonly ILogger<GetCashLedgerDetailReportUseCase> _logger;

    public GetCashLedgerDetailReportUseCase(
        ICashLedgerRepository repo,
        IReceiptRepository receiptRepo,
        IPaymentRepository paymentRepo,
        ILogger<GetCashLedgerDetailReportUseCase> logger)
    {
        _repo        = repo;
        _receiptRepo = receiptRepo;
        _paymentRepo = paymentRepo;
        _logger      = logger;
    }

    // 1 dòng báo cáo trước khi lọc/sắp xếp. Order = thứ tự dòng trong phiếu; CreatedAt = lúc lập phiếu.
    private sealed record Line(CashLedgerDetailRowDto Row, DateTime CreatedAt, int Order);

    public async Task<CashLedgerDetailReportDto> ExecuteAsync(
        DateTime from, DateTime to, IReadOnlyCollection<string> accountCodes,
        bool mergeSimilar, bool orderByCreated, CancellationToken ct = default)
    {
        _logger.LogInformation("Cash ledger detail report {From} → {To}, accounts [{Accounts}]",
            from, to, string.Join(",", accountCodes));

        var transactions = await _repo.GetUpToDateAsync(to, ct);
        var receipts = await _receiptRepo.GetByDocumentNumbersAsync(
            transactions.Where(t => t.ReceiptNumber != null).Select(t => t.ReceiptNumber!), ct);
        var payments = await _paymentRepo.GetByDocumentNumbersAsync(
            transactions.Where(t => t.PaymentNumber != null).Select(t => t.PaymentNumber!), ct);

        var lines = Expand(transactions, receipts, payments)
            .Where(l => IsSelected(l.Row.Account, accountCodes))
            .ToList();

        var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        // Số dư đầu kỳ gốc thuộc TK 111.
        var opening = (IsSelected(CashAccountPrefix, accountCodes) ? _repo.InitialBalance : 0m)
                      + lines.Where(l => l.Row.AccountingDate < utcFrom)
                             .Sum(l => l.Row.DebitAmount - l.Row.CreditAmount);

        var inPeriod = lines.Where(l => l.Row.AccountingDate >= utcFrom).ToList();
        if (mergeSimilar) inPeriod = Merge(inPeriod);

        var ordered = orderByCreated
            ? inPeriod.OrderBy(l => l.CreatedAt).ThenBy(l => DocumentNumber(l.Row)).ThenBy(l => l.Order)
            : inPeriod.OrderBy(l => l.Row.AccountingDate).ThenBy(l => l.Row.DocumentDate)
                      .ThenBy(l => l.CreatedAt).ThenBy(l => DocumentNumber(l.Row)).ThenBy(l => l.Order);

        var rows = ordered.Select(l => l.Row).ToList();
        var balance = opening;
        foreach (var row in rows)
        {
            balance += row.DebitAmount - row.CreditAmount;
            row.Balance = balance;
        }

        return new CashLedgerDetailReportDto
        {
            FromDate       = from,
            ToDate         = to,
            OpeningBalance = opening,
            ClosingBalance = balance,
            TotalDebit     = rows.Sum(r => r.DebitAmount),
            TotalCredit    = rows.Sum(r => r.CreditAmount),
            Rows           = rows,
        };
    }

    private static string DocumentNumber(CashLedgerDetailRowDto row) => row.ReceiptNumber ?? row.PaymentNumber ?? "";

    // Một dòng thuộc TK tiền mặt (111*) được tính khi TK của nó hoặc TK cha được chọn (chọn 111 là gồm cả
    // 1111). Không chọn gì = mọi TK tiền mặt. Tiền gửi (112*) không bao giờ vào báo cáo quỹ tiền mặt.
    private static bool IsSelected(string account, IReadOnlyCollection<string> selected) =>
        account.StartsWith(CashAccountPrefix, StringComparison.Ordinal)
        && (selected.Count == 0 || selected.Any(s => s.Length > 0 && account.StartsWith(s, StringComparison.Ordinal)));

    private static IEnumerable<Line> Expand(
        List<CashTransaction> transactions,
        Dictionary<string, Receipt> receipts,
        Dictionary<string, Payment> payments)
    {
        // Phiếu gốc chỉ bung 1 lần dù sổ quỹ có nhiều dòng trùng số.
        var expanded = new HashSet<string>();

        foreach (var t in transactions)
        {
            if (t.ReceiptNumber != null && receipts.TryGetValue(t.ReceiptNumber, out var receipt) && receipt.Entries.Count > 0)
            {
                if (!expanded.Add("PT:" + t.ReceiptNumber)) continue;
                var order = 0;
                foreach (var e in receipt.Entries.OrderBy(e => e.Id))
                {
                    yield return new Line(new CashLedgerDetailRowDto
                    {
                        AccountingDate = t.AccountingDate,
                        DocumentDate   = t.DocumentDate,
                        ReceiptNumber  = t.ReceiptNumber,
                        Description    = e.Description,
                        Account        = e.DebitAccountSetting?.Code ?? t.Account,
                        CounterAccount = e.CreditAccountSetting?.Code ?? t.CounterAccount,
                        DebitAmount    = e.Amount,
                        PersonName     = receipt.IsBulk && receipt.PayerName == CreateBulkCustomerReceiptUseCase.DefaultPayerName
                            ? null
                            : receipt.PayerName,
                        ReceiptId      = receipt.Id,
                        IsBulkReceipt  = receipt.IsBulk,
                    }, receipt.CreatedAt, order++);
                }
            }
            else if (t.PaymentNumber != null && payments.TryGetValue(t.PaymentNumber, out var payment) && payment.Entries.Count > 0)
            {
                if (!expanded.Add("PC:" + t.PaymentNumber)) continue;
                var order = 0;
                foreach (var e in payment.Entries.OrderBy(e => e.Id))
                {
                    yield return new Line(new CashLedgerDetailRowDto
                    {
                        AccountingDate = t.AccountingDate,
                        DocumentDate   = t.DocumentDate,
                        PaymentNumber  = t.PaymentNumber,
                        Description    = e.Description,
                        // Sổ quỹ ghi phiếu chi vào TK tiền mặt kể cả khi TK Có của dòng không phải 111*
                        // (xem ConfirmPaymentUseCase) — giữ đúng TK của sổ quỹ trong trường hợp đó.
                        Account        = e.CreditAccountSetting?.Code is { } code && code.StartsWith(CashAccountPrefix, StringComparison.Ordinal)
                            ? code
                            : t.Account,
                        CounterAccount = e.DebitAccountSetting?.Code ?? t.CounterAccount,
                        CreditAmount   = e.Amount,
                        PersonName     = payment.PayeeName,
                        CategoryCode   = e.ExpenseCategory?.Code,
                        CategoryName   = e.ExpenseCategory?.Name,
                        PaymentId      = payment.Id,
                    }, payment.CreatedAt, order++);
                }
            }
            else
            {
                yield return new Line(new CashLedgerDetailRowDto
                {
                    AccountingDate = t.AccountingDate,
                    DocumentDate   = t.DocumentDate,
                    ReceiptNumber  = t.ReceiptNumber,
                    PaymentNumber  = t.PaymentNumber,
                    Description    = t.Description,
                    Account        = t.Account,
                    CounterAccount = t.CounterAccount,
                    DebitAmount    = t.DebitAmount,
                    CreditAmount   = t.CreditAmount,
                    PersonName     = t.PersonName,
                }, t.CreatedAt, t.Id);
            }
        }
    }

    // "Cộng gộp các bút toán giống nhau": các dòng cùng phiếu + cùng diễn giải + cùng TK / TK đối ứng
    // gộp thành 1 dòng cộng tiền (giữ vị trí của dòng đầu).
    private static List<Line> Merge(List<Line> lines) =>
        lines.GroupBy(l => (l.Row.ReceiptNumber, l.Row.PaymentNumber, l.Row.AccountingDate,
                            l.Row.Description, l.Row.Account, l.Row.CounterAccount))
             .Select(g =>
             {
                 var first = g.First();
                 first.Row.DebitAmount  = g.Sum(l => l.Row.DebitAmount);
                 first.Row.CreditAmount = g.Sum(l => l.Row.CreditAmount);
                 return first;
             })
             .ToList();
}
