using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface ICashLedgerRepository
{
    Task<List<CashTransaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetBalanceBeforeDateAsync(DateTime date, CancellationToken ct = default);

    // Số dư đầu kỳ gốc của quỹ tiền mặt (TK 111) — đã nằm sẵn trong GetBalanceBeforeDateAsync.
    decimal InitialBalance { get; }

    // Mọi dòng sổ quỹ có ngày hạch toán <= to (kể cả trước kỳ) — báo cáo "Sổ kế toán chi tiết quỹ tiền
    // mặt" cần cả phần trước kỳ để tính Số tồn đầu kỳ theo đúng các tài khoản được chọn.
    Task<List<CashTransaction>> GetUpToDateAsync(DateTime to, CancellationToken ct = default);
    Task<CashTransaction> AddAsync(CashTransaction tx, CancellationToken ct = default);
    Task DeleteByReceiptNumberAsync(string receiptNumber, CancellationToken ct = default);
    Task DeleteByPaymentNumberAsync(string paymentNumber, CancellationToken ct = default);
}
