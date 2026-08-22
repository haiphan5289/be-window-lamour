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
                     && (!hasWarehouseFilter || (l.WarehouseId.HasValue && warehouseIds!.Contains(l.WarehouseId.Value))))
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

    public async Task<IEnumerable<(
        DateTime AccountingDate, DateTime DocumentDate, string DocumentNumber, string DocumentType,
        int? SourceId, string? Description, string Unit,
        int ImportQty, decimal ImportValue, int ExportQty)>> GetTransactionLinesByProductAsync(
        int productId, DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hasWarehouseFilter = warehouseIds is { Count: > 0 };

        var imports = await _db.WarehouseReceiptLines
            .AsNoTracking()
            .Where(l => l.ProductId == productId
                     && l.WarehouseReceipt.Status == WarehouseReceiptStatus.Confirmed
                     && l.WarehouseReceipt.AccountingDate >= fromUtc
                     && l.WarehouseReceipt.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || warehouseIds!.Contains(l.WarehouseId)))
            .Select(l => new
            {
                AccountingDate = l.WarehouseReceipt.AccountingDate,
                DocumentDate   = l.WarehouseReceipt.DocumentDate,
                DocumentNumber = l.WarehouseReceipt.ReceiptNumber,
                SourceId       = l.WarehouseReceipt.Id,
                Description    = l.WarehouseReceipt.Description,
                Unit           = l.Product.Unit,
                Qty            = l.Quantity,
                Value          = l.Amount,
            })
            .ToListAsync(ct);

        var exports = await _db.SalesOrderLines
            .AsNoTracking()
            .Where(l => l.ProductId == productId
                     && !l.IsPromotion
                     && l.SalesOrder.AccountingDate >= fromUtc
                     && l.SalesOrder.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || (l.WarehouseId.HasValue && warehouseIds!.Contains(l.WarehouseId.Value))))
            .Select(l => new
            {
                AccountingDate = l.SalesOrder.AccountingDate,
                DocumentDate   = l.SalesOrder.DocumentDate,
                DocumentNumber = l.SalesOrder.DocumentNumber,
                SourceId       = l.SalesOrder.Id,
                Description    = "Xuất kho bán hàng " + (l.SalesOrder.Customer != null ? l.SalesOrder.Customer.Name : ""),
                Unit           = l.Unit,
                Qty            = l.Quantity,
            })
            .ToListAsync(ct);

        var returns = await _db.SalesReturnLines
            .AsNoTracking()
            .Where(l => l.ProductId == productId
                     && l.SalesReturn.AccountingDate >= fromUtc
                     && l.SalesReturn.AccountingDate <  toUtc
                     && (!hasWarehouseFilter || warehouseIds!.Contains(l.WarehouseId)))
            .Select(l => new
            {
                AccountingDate = l.SalesReturn.AccountingDate,
                DocumentDate   = l.SalesReturn.DocumentDate,
                DocumentNumber = l.SalesReturn.DocumentNumber,
                Description    = "Hàng bán bị trả lại " + (l.SalesReturn.Customer != null ? l.SalesReturn.Customer.Name : ""),
                Unit           = l.Unit,
                Qty            = l.Quantity,
            })
            .ToListAsync(ct);

        var rows = new List<(DateTime, DateTime, string, string, int?, string?, string, int, decimal, int)>();

        foreach (var i in imports)
            rows.Add((i.AccountingDate, i.DocumentDate, i.DocumentNumber, "Import", i.SourceId, i.Description, i.Unit, i.Qty, i.Value, 0));

        foreach (var e in exports)
            rows.Add((e.AccountingDate, e.DocumentDate, e.DocumentNumber, "Export", e.SourceId, e.Description, e.Unit, 0, 0m, e.Qty));

        // Hàng bán bị trả lại làm TĂNG tồn kho (giống 1 lần Nhập) — không có SourceId (WPF chưa có
        // màn xem lại chứng từ trả hàng từ đây, chỉ hiện text) — khớp GetExportQtyByProductAsync
        // vốn cũng trừ returnRows khỏi export qty, không xử lý riêng.
        foreach (var r in returns)
            rows.Add((r.AccountingDate, r.DocumentDate, r.DocumentNumber, "SalesReturn", null, r.Description, r.Unit, r.Qty, 0m, 0));

        return rows.OrderBy(r => r.Item1).ThenBy(r => r.Item3);
    }
}
