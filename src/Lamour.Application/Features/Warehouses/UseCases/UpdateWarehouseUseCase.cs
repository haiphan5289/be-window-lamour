using Lamour.Application.Abstractions;
using Lamour.Application.Features.Warehouses.Dtos;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouses.UseCases;

public class UpdateWarehouseUseCase : IUpdateWarehouseUseCase
{
    private readonly IWarehouseRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateWarehouseUseCase> _logger;

    public UpdateWarehouseUseCase(IWarehouseRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateWarehouseUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<WarehouseResponseDto> ExecuteAsync(int id, UpdateWarehouseRequestDto request, CancellationToken ct = default)
    {
        var warehouse = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Warehouse {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Mã kho không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên kho không được để trống.");

        var code = request.Code.Trim();
        if (await _repo.CodeExistsAsync(code, excludeId: id, ct: ct))
            throw new DomainException($"Kho '{code}' đã tồn tại.");

        warehouse.Code     = code;
        warehouse.Name     = request.Name.Trim();
        warehouse.IsActive = request.IsActive;
        var updated = await _repo.UpdateAsync(warehouse, ct);
        _logger.LogInformation("Updated warehouse {Id}", id);

        var dto = GetWarehousesUseCase.MapToDto(updated);
        await _broadcaster.WarehouseUpdatedAsync(dto, ct);
        return dto;
    }
}
