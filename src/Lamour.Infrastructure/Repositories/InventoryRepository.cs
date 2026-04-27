using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;

    public InventoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken ct = default)
        => await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(ct);

    public async Task<Dictionary<int, (int Qty, decimal Value)>> GetImportsByProductAsync(
        DateOnly fromDate, DateOnly toDate, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var rows = await _db.WarehouseReceiptLines
            .AsNoTracking()
            .Where(l => l.WarehouseReceipt.Status == WarehouseReceiptStatus.Confirmed
                     && l.WarehouseReceipt.AccountingDate >= fromUtc
                     && l.WarehouseReceipt.AccountingDate <  toUtc)
            .GroupBy(l => l.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty       = g.Sum(l => l.Quantity),
                Value     = g.Sum(l => l.Amount),
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.ProductId, r => (r.Qty, r.Value));
    }
}
