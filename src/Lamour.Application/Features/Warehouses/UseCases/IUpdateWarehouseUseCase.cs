using Lamour.Application.Features.Warehouses.Dtos;

namespace Lamour.Application.Features.Warehouses.UseCases;

public interface IUpdateWarehouseUseCase
{
    Task<WarehouseResponseDto> ExecuteAsync(int id, UpdateWarehouseRequestDto request, CancellationToken ct = default);
}
