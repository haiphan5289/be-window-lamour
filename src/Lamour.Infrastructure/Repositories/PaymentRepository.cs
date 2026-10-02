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

    public async Task<Dictionary<int, string>> GetReasonDetailsByIdsAsync(
        IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<int, string>();

        var rows = await _db.Payments
            .AsNoTracking()
            .Where(x => idList.Contains(x.Id) && x.ReasonDetail != null && x.ReasonDetail != "")
            .Select(x => new { x.Id, x.ReasonDetail })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => r.ReasonDetail!);
    }

    public async Task<Dictionary<string, Payment>> GetByDocumentNumbersAsync(
        IEnumerable<string> documentNumbers, CancellationToken ct = default)
    {
        var numbers = documentNumbers.Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        if (numbers.Count == 0) return new Dictionary<string, Payment>();

        var rows = await _db.Payments
            .AsNoTracking()
            .Where(x => numbers.Contains(x.DocumentNumber))
            .Include(p => p.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(p => p.Entries).ThenInclude(e => e.CreditAccountSetting)
            .Include(p => p.Entries).ThenInclude(e => e.ExpenseCategory)
            .ToListAsync(ct);
        return rows.GroupBy(r => r.DocumentNumber).ToDictionary(g => g.Key, g => g.OrderBy(r => r.Id).Last());
    }

    public async Task<Dictionary<string, int>> GetIdsByDocumentNumbersAsync(
        IEnumerable<string> documentNumbers, CancellationToken ct = default)
    {
        var numbers = documentNumbers.Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        if (numbers.Count == 0) return new Dictionary<string, int>();

        // Số chứng từ trùng (hiếm, do user tự nhập) → lấy phiếu tạo sau cùng.
        var rows = await _db.Payments
            .AsNoTracking()
            .Where(x => numbers.Contains(x.DocumentNumber))
            .Select(x => new { x.DocumentNumber, x.Id })
            .ToListAsync(ct);
        return rows.GroupBy(r => r.DocumentNumber).ToDictionary(g => g.Key, g => g.Max(r => r.Id));
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
