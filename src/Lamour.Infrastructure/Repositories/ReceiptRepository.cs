using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDbContext _db;

    public ReceiptRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Receipt>> GetAllAsync(CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<Receipt?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        // Reload navigations
        await _db.Entry(receipt).Reference(r => r.Customer).LoadAsync(ct);
        if (receipt.CollectorEmployeeId.HasValue)
            await _db.Entry(receipt).Reference(r => r.CollectorEmployee).LoadAsync(ct);

        return receipt;
    }

    public async Task UpdateAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Update(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Remove(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetNextCodeNumberAsync(string prefix, CancellationToken ct = default)
    {
        var numbers = await _db.Receipts
            .AsNoTracking()
            .Select(r => r.DocumentNumber)
            .Where(n => n.StartsWith(prefix))
            .ToListAsync(ct);

        var max = numbers
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return max + 1;
    }

    public async Task<decimal> GetRemainingAmountAsync(int salesOrderId, CancellationToken ct = default)
    {
        var order = await _db.SalesOrders
            .AsNoTracking()
            .Where(o => o.Id == salesOrderId)
            .Select(o => (decimal?)o.GrandTotal)
            .FirstOrDefaultAsync(ct);
        if (order is null) return 0m;

        var paid = await _db.ReceiptEntries
            .AsNoTracking()
            .Where(e => e.SalesOrderId == salesOrderId)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        var deducted = await _db.DepositDeductions
            .AsNoTracking()
            .Where(d => d.SalesOrderId == salesOrderId)
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        return order.Value - paid - deducted;
    }

    public async Task<IEnumerable<(
        int OrderId, string DocumentNumber, DateTime AccountingDate, DateTime DocumentDate,
        int CustomerId, string CustomerCode, string CustomerName, string? Description,
        decimal RemainingAmount)>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var rows = await _db.SalesOrders
            .AsNoTracking()
            .Where(o => o.Status == SalesOrderStatus.Normal
                     && o.AccountingDate >= fromUtc
                     && o.AccountingDate <  toUtc
                     && (!employeeId.HasValue || o.EmployeeId == employeeId.Value))
            .Select(o => new
            {
                o.Id,
                o.DocumentNumber,
                o.AccountingDate,
                o.DocumentDate,
                o.CustomerId,
                CustomerCode = o.Customer.Code,
                CustomerName = o.Customer.Name,
                o.Description,
                o.GrandTotal,
                Paid     = _db.ReceiptEntries.Where(e => e.SalesOrderId == o.Id).Sum(e => (decimal?)e.Amount) ?? 0m,
                Deducted = _db.DepositDeductions.Where(d => d.SalesOrderId == o.Id).Sum(d => (decimal?)d.Amount) ?? 0m,
            })
            .ToListAsync(ct);

        return rows
            .Select(r => (
                r.Id, r.DocumentNumber, r.AccountingDate, r.DocumentDate,
                r.CustomerId, r.CustomerCode, r.CustomerName, r.Description,
                RemainingAmount: r.GrandTotal - r.Paid - r.Deducted))
            .Where(r => r.RemainingAmount > 0m)
            .OrderBy(r => r.AccountingDate).ThenBy(r => r.DocumentNumber);
    }
}
