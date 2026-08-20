using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class SalesReturnRepository : ISalesReturnRepository
{
    private readonly AppDbContext _db;

    public SalesReturnRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<SalesReturn>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.SalesReturns
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.DocumentDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(r => r.DocumentDate < to);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search}%";
            query = query.Where(r =>
                EF.Functions.ILike(r.DocumentNumber, term) ||
                (r.Customer != null && EF.Functions.ILike(r.Customer.Name, term)) ||
                (r.Employee != null && EF.Functions.ILike(r.Employee.Name, term)));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<SalesReturn?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.SalesReturns
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<SalesReturn?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.SalesReturns
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<SalesReturn> AddAsync(SalesReturn salesReturn, CancellationToken ct = default)
    {
        _db.SalesReturns.Add(salesReturn);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(salesReturn).Reference(r => r.Customer).LoadAsync(ct);
        if (salesReturn.EmployeeId.HasValue)
            await _db.Entry(salesReturn).Reference(r => r.Employee).LoadAsync(ct);

        return salesReturn;
    }

    public async Task UpdateAsync(SalesReturn salesReturn, CancellationToken ct = default)
    {
        _db.SalesReturns.Update(salesReturn);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(SalesReturn salesReturn, CancellationToken ct = default)
    {
        _db.SalesReturns.Remove(salesReturn);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<IEnumerable<SalesReturnLine>> GetReportLinesAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = _db.SalesReturnLines
            .AsNoTracking()
            .Include(l => l.SalesReturn).ThenInclude(r => r.Customer)
            .Include(l => l.SalesReturn).ThenInclude(r => r.Employee)
            .Include(l => l.Product).ThenInclude(p => p.Category)
            .AsQueryable();

        var productIdList = productIds?.ToList();
        if (productIdList is { Count: > 0 })
            query = query.Where(l => productIdList.Contains(l.ProductId));
        if (employeeId.HasValue)
            query = query.Where(l => l.SalesReturn.EmployeeId == employeeId.Value);
        if (customerId.HasValue)
            query = query.Where(l => l.SalesReturn.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(unit))
            query = query.Where(l => l.Unit == unit);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(l => l.Product.Category != null && l.Product.Category.Name == category);
        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.SalesReturn.AccountingDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.SalesReturn.AccountingDate <= to);
        }

        return await query
            .OrderByDescending(l => l.SalesReturn.AccountingDate)
            .ToListAsync(ct);
    }

    public async Task<int> GetNextCodeNumberAsync(CancellationToken ct = default)
    {
        const string prefix = "BTL";
        var numbers = await _db.SalesReturns
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
}
