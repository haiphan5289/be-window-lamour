using Lamour.Application.Features.Warehouse.Dtos;

namespace Lamour.Application.Features.Warehouse.UseCases;

public interface IGetInventorySummaryUseCase
{
    Task<IEnumerable<InventorySummaryItemDto>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}
