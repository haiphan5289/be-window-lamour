using Lamour.Application.Features.Warehouse.Dtos;
using Lamour.Application.Features.Warehouse.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouse.UseCases;

// Tổng hợp tồn kho chia theo kho (khớp MISA "Tên kho : Hàng Hóa (66)"). Cùng công thức với
// GetInventorySummaryUseCase nhưng tính trên từng cặp (kho, sản phẩm):
//   Opening = Closing − Import + Export; Value mở/đóng/xuất = Qty × Product.CostPrice; ImportValue = Amount thật trên phiếu nhập.
public class GetInventorySummaryByWarehouseUseCase : IGetInventorySummaryByWarehouseUseCase
{
    private readonly IInventoryRepository _repo;
    private readonly ILogger<GetInventorySummaryByWarehouseUseCase> _logger;

    public GetInventorySummaryByWarehouseUseCase(
        IInventoryRepository repo, ILogger<GetInventorySummaryByWarehouseUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<InventorySummaryByWarehouseDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching inventory summary by warehouse from {From} to {To}, warehouses={WarehouseIds}",
            fromDate, toDate, warehouseIds is { Count: > 0 } ? string.Join(",", warehouseIds) : "all");

        var products = await _repo.GetAllAsync(ct);
        if (categoryId is not null)
            products = products.Where(p => p.CategoryId == categoryId.Value);
        if (productUnitId is not null)
            products = products.Where(p => p.ProductUnitId == productUnitId.Value);
        if (productIds is { Count: > 0 })
            products = products.Where(p => productIds.Contains(p.Id));
        var productList = products.ToList();

        var warehouses  = await _repo.GetWarehousesAsync(warehouseIds, ct);
        var imports     = await _repo.GetImportsByWarehouseProductAsync(fromDate, toDate, warehouseIds, ct);
        var exportQtys  = await _repo.GetExportQtyByWarehouseProductAsync(fromDate, toDate, warehouseIds, ct);
        var closingQtys = await _repo.GetClosingQtyByWarehouseProductAsync(warehouseIds, ct: ct);

        var groups = new List<InventorySummaryByWarehouseDto>();

        foreach (var w in warehouses.OrderBy(w => w.Name, StringComparer.OrdinalIgnoreCase))
        {
            var items = new List<InventorySummaryItemDto>();

            foreach (var p in productList)
            {
                var key = (w.Id, p.Id);
                imports.TryGetValue(key, out var imp);
                exportQtys.TryGetValue(key, out var exportQty);
                closingQtys.TryGetValue(key, out var closingQty);

                var importQty  = imp.Qty;
                var openingQty = closingQty - importQty + exportQty;

                if (openingQty == 0 && importQty == 0 && exportQty == 0 && closingQty == 0)
                    continue;

                items.Add(new InventorySummaryItemDto
                {
                    ProductId            = p.Id,
                    Code                 = p.Code,
                    Name                 = p.Name,
                    Unit                 = p.Unit,
                    OpeningQty           = openingQty,
                    OpeningValue         = openingQty * p.CostPrice,
                    ImportQty            = importQty,
                    ImportValue          = imp.Value,
                    ExportQty            = exportQty,
                    ExportValue          = exportQty * p.CostPrice,
                    ClosingQty           = closingQty,
                    ClosingValue         = closingQty * p.CostPrice,
                    LatestAccountingDate = imp.LatestDate,
                });
            }

            if (items.Count == 0) continue;

            groups.Add(new InventorySummaryByWarehouseDto
            {
                WarehouseId   = w.Id,
                WarehouseCode = w.Code,
                WarehouseName = w.Name,
                Items         = items,
            });
        }

        return groups;
    }
}
