using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _db.Payments
            .AsNoTracking()
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries).ThenInclude(e => e.ExpenseCategory)
            .Include(p => p.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(p => p.Entries).ThenInclude(e => e.CreditAccountSetting)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Payment>> GetUnconfirmedByDateRangeAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);
        return await _db.Payments
            .AsNoTracking()
            .Where(p => p.Status != PaymentStatus.Confirmed
                     && p.AccountingDate >= utcFrom
                     && p.AccountingDate <= utcTo)
            .Include(p => p.Entries).ThenInclude(e => e.DebitAccountSetting)
            .OrderBy(p => p.AccountingDate)
            .ThenBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Payments
            .AsNoTracking()
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries).ThenInclude(e => e.ExpenseCategory)
            .Include(p => p.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(p => p.Entries).ThenInclude(e => e.CreditAccountSetting)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Payments
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries).ThenInclude(e => e.ExpenseCategory)
            .Include(p => p.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(p => p.Entries).ThenInclude(e => e.CreditAccountSetting)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        // Reload navigations
        if (payment.PaymentEmployeeId.HasValue)
            await _db.Entry(payment).Reference(p => p.PaymentEmployee).LoadAsync(ct);
        foreach (var entry in payment.Entries)
        {
            await _db.Entry(entry).Reference(e => e.DebitAccountSetting).LoadAsync(ct);
            await _db.Entry(entry).Reference(e => e.CreditAccountSetting).LoadAsync(ct);
            if (entry.ExpenseCategoryId.HasValue)
                await _db.Entry(entry).Reference(e => e.ExpenseCategory).LoadAsync(ct);
        }

        return payment;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync(ct);
    }
}
