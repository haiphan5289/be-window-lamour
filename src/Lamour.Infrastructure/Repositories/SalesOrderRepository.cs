using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly AppDbContext _db;

    public SalesOrderRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<SalesOrder>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines.OrderBy(l => l.Id))
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.DocumentDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(o => o.DocumentDate < to);
        }
        // ILike (Npgsql) thay vì Contains() để khớp hành vi case-insensitive (OrdinalIgnoreCase)
        // mà SalesOrderListViewModel.Matches() đang làm phía client trước khi chuyển filter xuống
        // SQL. Không lọc theo StatusLabel (nhãn tiếng Việt tính ở WPF, không map 1-1 xuống DB) —
        // đổi nhỏ so với hành vi client-side cũ, chấp nhận được.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search}%";
            query = query.Where(o =>
                EF.Functions.ILike(o.DocumentNumber, term) ||
                (o.Customer != null && EF.Functions.ILike(o.Customer.Name, term)) ||
                (o.Employee != null && EF.Functions.ILike(o.Employee.Name, term)) ||
                (o.Notes != null && EF.Functions.ILike(o.Notes, term)));
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines.OrderBy(l => l.Id))
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<SalesOrder?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Lines.OrderBy(l => l.Id))
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

    public async Task<IEnumerable<SalesOrderLine>> GetReportLinesAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = _db.SalesOrderLines
            .AsNoTracking()
            .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
            .Include(l => l.SalesOrder).ThenInclude(o => o.Employee)
            .Include(l => l.Product).ThenInclude(p => p.Category)
            .AsQueryable();

        var productIdList = productIds?.ToList();
        if (productIdList is { Count: > 0 })
            query = query.Where(l => productIdList.Contains(l.ProductId));
        if (employeeId.HasValue)
            query = query.Where(l => l.SalesOrder.EmployeeId == employeeId.Value);
        if (customerId.HasValue)
            query = query.Where(l => l.SalesOrder.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(unit))
            query = query.Where(l => l.Unit == unit);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(l => l.Product.Category != null && l.Product.Category.Name == category);
        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.SalesOrder.AccountingDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.SalesOrder.AccountingDate <= to);
        }

        return await query
            .OrderByDescending(l => l.SalesOrder.AccountingDate)
            .ToListAsync(ct);
    }

    public async Task<int> GetNextCodeNumberAsync(string prefix, CancellationToken ct = default)
    {
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
