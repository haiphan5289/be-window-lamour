using Lamour.Application.Features.Warehouses.Dtos;

namespace Lamour.Application.Features.Warehouses.UseCases;

public interface IGetWarehousesUseCase
{
    Task<IEnumerable<WarehouseResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
