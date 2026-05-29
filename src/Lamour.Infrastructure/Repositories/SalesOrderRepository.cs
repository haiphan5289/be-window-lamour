using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly AppDbContext _db;

    public SalesOrderRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<SalesOrder>> GetAllAsync(CancellationToken ct = default)
        => await _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<SalesOrder?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken ct = default)
    {
        _db.SalesOrders.Add(order);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(order).Reference(o => o.Customer).LoadAsync(ct);
        if (order.EmployeeId.HasValue)
            await _db.Entry(order).Reference(o => o.Employee).LoadAsync(ct);

        return order;
    }

    public async Task UpdateAsync(SalesOrder order, CancellationToken ct = default)
    {
        _db.SalesOrders.Update(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(SalesOrder order, CancellationToken ct = default)
    {
        _db.SalesOrders.Remove(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<int> GetNextCodeNumberAsync(CancellationToken ct = default)
    {
        const string prefix = "BC";
        var numbers = await _db.SalesOrders
            .AsNoTracking()
            .Select(o => o.DocumentNumber)
            .Where(n => n.StartsWith(prefix))
            .ToListAsync(ct);

        var max = numbers
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return max + 1;
    }
}
