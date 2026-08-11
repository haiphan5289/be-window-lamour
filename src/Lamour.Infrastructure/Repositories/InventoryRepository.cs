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

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        => await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .ToListAsync(ct);

    public async Task<Dictionary<int, (int Qty, decimal Value, DateTime? LatestDate)>> GetImportsByProductAsync(
        DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hasWarehouseFilter = warehouseIds is { Count: > 0 };

        var rows = await _db.WarehouseReceiptLines
            .AsNoTracking()
            .Where(l => l.WarehouseReceipt.Status == WarehouseReceiptStatus.Confirmed
                     && l.WarehouseReceipt.AccountingDate >= fromUtc
                     && l.WarehouseReceipt.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || warehouseIds!.Contains(l.WarehouseId)))
            .GroupBy(l => l.ProductId)
            .Select(g => new
            {
                ProductId  = g.Key,
                Qty        = g.Sum(l => l.Quantity),
                Value      = g.Sum(l => l.Amount),
                LatestDate = (DateTime?)g.Max(l => l.WarehouseReceipt.AccountingDate),
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.ProductId, r => (r.Qty, r.Value, r.LatestDate));
    }

    public async Task<Dictionary<int, int>> GetExportQtyByProductAsync(
        DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hasWarehouseFilter = warehouseIds is { Count: > 0 };

        var salesRows = await _db.SalesOrderLines
            .AsNoTracking()
            .Where(l => !l.IsPromotion
                     && l.SalesOrder.AccountingDate >= fromUtc
                     && l.SalesOrder.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || warehouseIds!.Contains(l.WarehouseId)))
            .GroupBy(l => l.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(l => l.Quantity) })
            .ToListAsync(ct);

        var returnRows = await _db.SalesReturnLines
            .AsNoTracking()
            .Where(l => l.SalesReturn.AccountingDate >= fromUtc
                     && l.SalesReturn.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || warehouseIds!.Contains(l.WarehouseId)))
            .GroupBy(l => l.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(l => l.Quantity) })
            .ToListAsync(ct);

        var result = salesRows.ToDictionary(r => r.ProductId, r => r.Qty);
        foreach (var r in returnRows)
            result[r.ProductId] = result.TryGetValue(r.ProductId, out var existing) ? existing - r.Qty : -r.Qty;

        return result;
    }

    public async Task<Dictionary<int, int>> GetClosingQtyByProductAsync(
        IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default)
    {
        var query = _db.ProductWarehouseStocks.AsNoTracking().AsQueryable();
        if (warehouseIds is { Count: > 0 })
            query = query.Where(x => warehouseIds.Contains(x.WarehouseId));

        var rows = await query
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.ProductId, r => r.Qty);
    }
}
