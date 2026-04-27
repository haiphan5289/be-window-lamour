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
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching inventory summary from {From} to {To}", fromDate, toDate);

        var products = await _repo.GetAllActiveAsync(ct);
        var imports  = await _repo.GetImportsByProductAsync(fromDate, toDate, ct);

        return products.Select(p =>
        {
            imports.TryGetValue(p.Id, out var imp);
            var importQty    = imp.Qty;
            var importValue  = imp.Value;
            var closingQty   = p.StockQuantity;
            var openingQty   = closingQty - importQty;   // ExportQty not yet tracked
            var openingValue = openingQty * p.CostPrice;
            var closingValue = closingQty * p.CostPrice;

            return new InventorySummaryItemDto
            {
                ProductId    = p.Id,
                Code         = p.Code,
                Name         = p.Name,
                Unit         = p.Unit,
                OpeningQty   = openingQty,
                OpeningValue = openingValue,
                ImportQty    = importQty,
                ImportValue  = importValue,
                ExportQty    = 0,
                ExportValue  = 0,
                ClosingQty   = closingQty,
                ClosingValue = closingValue,
            };
        });
    }
}
