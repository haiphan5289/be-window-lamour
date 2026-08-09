using Lamour.Application.Features.Warehouses.Dtos;
using Lamour.Application.Features.Warehouses.Repositories;
using Microsoft.Extensions.Logging;
using WarehouseEntity = Lamour.Domain.Entities.Warehouse;

namespace Lamour.Application.Features.Warehouses.UseCases;

public class GetWarehousesUseCase : IGetWarehousesUseCase
{
    private readonly IWarehouseRepository _repo;
    private readonly ILogger<GetWarehousesUseCase> _logger;

    public GetWarehousesUseCase(IWarehouseRepository repo, ILogger<GetWarehousesUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all warehouses");
        var warehouses = await _repo.GetAllAsync(ct);
        return warehouses.Select(MapToDto);
    }

    internal static WarehouseResponseDto MapToDto(WarehouseEntity w) => new()
    {
        Id       = w.Id,
        Code     = w.Code,
        Name     = w.Name,
        IsActive = w.IsActive,
    };
}
