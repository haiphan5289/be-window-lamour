using Lamour.Application.Features.Warehouse.Dtos;

namespace Lamour.Application.Features.Warehouse.UseCases;

public interface IGetInventorySummaryUseCase
{
    Task<IEnumerable<InventorySummaryItemDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        CancellationToken ct = default);
}
