using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class CashLedgerRepository : ICashLedgerRepository
{
    private readonly AppDbContext _db;

    // Hardcoded initial balance (số dư đầu kỳ gốc)
    private const decimal InitialBalance = 129_501_061m;

    public CashLedgerRepository(AppDbContext db) => _db = db;

    public async Task<List<CashTransaction>> GetByDateRangeAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);
        return await _db.CashTransactions
            .AsNoTracking()
            .Where(c => c.AccountingDate >= utcFrom && c.AccountingDate <= utcTo)
            .OrderBy(c => c.AccountingDate)
            .ThenBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetBalanceBeforeDateAsync(
        DateTime date, CancellationToken ct = default)
    {
        var utcDate     = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        var netBeforeDate = await _db.CashTransactions
            .AsNoTracking()
            .Where(c => c.AccountingDate < utcDate)
            .SumAsync(c => c.DebitAmount - c.CreditAmount, ct);

        return InitialBalance + netBeforeDate;
    }

    public async Task<CashTransaction> AddAsync(CashTransaction tx, CancellationToken ct = default)
    {
        _db.CashTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        return tx;
    }
}
