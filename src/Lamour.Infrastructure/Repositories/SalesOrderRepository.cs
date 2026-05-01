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
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<SalesOrder?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
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
}
