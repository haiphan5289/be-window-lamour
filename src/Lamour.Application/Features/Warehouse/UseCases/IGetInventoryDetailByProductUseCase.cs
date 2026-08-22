using Lamour.Application.Features.Warehouse.Dtos;

namespace Lamour.Application.Features.Warehouse.UseCases;

public interface IGetInventoryDetailByProductUseCase
{
    Task<InventoryDetailResponseDto?> ExecuteAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default);
}
