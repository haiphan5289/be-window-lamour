using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface ICashLedgerRepository
{
    Task<List<CashTransaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetBalanceBeforeDateAsync(DateTime date, CancellationToken ct = default);
}
