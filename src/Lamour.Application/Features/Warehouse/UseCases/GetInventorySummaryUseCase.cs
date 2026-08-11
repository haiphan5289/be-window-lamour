using Lamour.Application.Features.Warehouse.Dtos;
using Lamour.Application.Features.Warehouse.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouse.UseCases;

public class GetInventorySummaryUseCase : IGetInventorySummaryUseCase
{
    private readonly IInventoryRepository _repo;
    private readonly ILogger<GetInventorySummaryUseCase> _logger;

    public GetInventorySummaryUseCase(IInventoryRepository repo, ILogger<GetInventorySummaryUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<InventorySummaryItemDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching inventory summary from {From} to {To}, warehouses={WarehouseIds}",
            fromDate, toDate, warehouseIds is { Count: > 0 } ? string.Join(",", warehouseIds) : "all");

        var products = await _repo.GetAllAsync(ct);
        if (categoryId is not null)
            products = products.Where(p => p.CategoryId == categoryId.Value);
        if (productUnitId is not null)
            products = products.Where(p => p.ProductUnitId == productUnitId.Value);

        var imports    = await _repo.GetImportsByProductAsync(fromDate, toDate, warehouseIds, ct);
        var exportQtys = await _repo.GetExportQtyByProductAsync(fromDate, toDate, warehouseIds, ct);
        var closingQtys = await _repo.GetClosingQtyByProductAsync(warehouseIds, ct);

        return products.Select(p =>
        {
            imports.TryGetValue(p.Id, out var imp);
            exportQtys.TryGetValue(p.Id, out var exportQty);
            closingQtys.TryGetValue(p.Id, out var closingQty);

            var importQty    = imp.Qty;
            var importValue  = imp.Value;
            // Closing = Opening + Import − Export  ⇒  Opening = Closing − Import + Export
            var openingQty   = closingQty - importQty + exportQty;
            var openingValue = openingQty * p.CostPrice;
            var exportValue  = exportQty * p.CostPrice;
            var closingValue = closingQty * p.CostPrice;

            return new InventorySummaryItemDto
            {
                ProductId            = p.Id,
                Code                 = p.Code,
                Name                 = p.Name,
                Unit                 = p.Unit,
                OpeningQty           = openingQty,
                OpeningValue         = openingValue,
                ImportQty            = importQty,
                ImportValue          = importValue,
                ExportQty            = exportQty,
                ExportValue          = exportValue,
                ClosingQty           = closingQty,
                ClosingValue         = closingValue,
                LatestAccountingDate = imp.LatestDate,
            };
        });
    }
}
