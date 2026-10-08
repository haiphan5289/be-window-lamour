using Lamour.Application.Features.Warehouse.Dtos;

namespace Lamour.Application.Features.Warehouse.UseCases;

public interface IGetInventorySummaryByWarehouseUseCase
{
    Task<IEnumerable<InventorySummaryByWarehouseDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default);
}
