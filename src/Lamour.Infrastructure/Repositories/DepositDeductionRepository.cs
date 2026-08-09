using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class DepositDeductionRepository : IDepositDeductionRepository
{
    private readonly AppDbContext _db;

    public DepositDeductionRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<DepositDeduction>> GetAllAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = _db.DepositDeductions
            .AsNoTracking()
            .Include(x => x.Deposit).ThenInclude(d => d.Customer)
            .Include(x => x.SalesOrder).ThenInclude(o => o.Employee)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(x => x.Deposit.CustomerId == customerId.Value);
        if (employeeId.HasValue)
            query = query.Where(x => x.SalesOrder.EmployeeId == employeeId.Value);
        if (salesOrderId.HasValue)
            query = query.Where(x => x.SalesOrderId == salesOrderId.Value);
        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.AccountingDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.AccountingDate <= to);
        }

        return await query
            .OrderByDescending(x => x.AccountingDate)
            .ToListAsync(ct);
    }

    public async Task<DepositDeduction?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.DepositDeductions
            .AsNoTracking()
            .Include(x => x.Deposit).ThenInclude(d => d.Customer)
            .Include(x => x.SalesOrder).ThenInclude(o => o.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<DepositDeduction?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.DepositDeductions
            .Include(x => x.Deposit)
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<DepositDeduction> AddAsync(DepositDeduction deduction, CancellationToken ct = default)
    {
        _db.DepositDeductions.Add(deduction);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(deduction).Reference(x => x.Deposit).LoadAsync(ct);
        await _db.Entry(deduction.Deposit).Reference(d => d.Customer).LoadAsync(ct);
        await _db.Entry(deduction).Reference(x => x.SalesOrder).LoadAsync(ct);
        if (deduction.SalesOrder.EmployeeId.HasValue)
            await _db.Entry(deduction.SalesOrder).Reference(o => o.Employee).LoadAsync(ct);

        return deduction;
    }

    public async Task DeleteAsync(DepositDeduction deduction, CancellationToken ct = default)
    {
        _db.DepositDeductions.Remove(deduction);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<int> GetNextCodeNumberAsync(CancellationToken ct = default)
    {
        const string prefix = "TC";
        var numbers = await _db.DepositDeductions
            .AsNoTracking()
            .Select(x => x.DocumentNumber)
            .Where(n => n.StartsWith(prefix))
            .ToListAsync(ct);

        var max = numbers
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return max + 1;
    }
}
