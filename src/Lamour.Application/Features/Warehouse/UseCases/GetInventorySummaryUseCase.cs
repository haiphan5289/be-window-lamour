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

        return products.Select(p => new InventorySummaryItemDto
        {
            ProductId    = p.Id,
            Code         = p.Code,
            Name         = p.Name,
            Unit         = p.Unit,
            OpeningQty   = 0,
            OpeningValue = 0,
            ImportQty    = 0,
            ImportValue  = 0,
            ExportQty    = 0,
            ExportValue  = 0,
            ClosingQty   = p.StockQuantity,
            ClosingValue = p.StockQuantity * p.CostPrice,
        });
    }
}
