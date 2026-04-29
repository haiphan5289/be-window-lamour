using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface ICashLedgerRepository
{
    Task<List<CashTransaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetBalanceBeforeDateAsync(DateTime date, CancellationToken ct = default);
    Task<CashTransaction> AddAsync(CashTransaction tx, CancellationToken ct = default);
    Task DeleteByReceiptNumberAsync(string receiptNumber, CancellationToken ct = default);
    Task DeleteByPaymentNumberAsync(string paymentNumber, CancellationToken ct = default);
}
