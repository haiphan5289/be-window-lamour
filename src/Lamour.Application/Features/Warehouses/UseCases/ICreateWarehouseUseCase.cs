using Lamour.Application.Features.Warehouses.Dtos;

namespace Lamour.Application.Features.Warehouses.UseCases;

public interface ICreateWarehouseUseCase
{
    Task<WarehouseResponseDto> ExecuteAsync(CreateWarehouseRequestDto request, CancellationToken ct = default);
}
