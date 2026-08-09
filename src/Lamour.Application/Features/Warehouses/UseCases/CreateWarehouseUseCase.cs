using Lamour.Application.Abstractions;
using Lamour.Application.Features.Warehouses.Dtos;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using WarehouseEntity = Lamour.Domain.Entities.Warehouse;

namespace Lamour.Application.Features.Warehouses.UseCases;

public class CreateWarehouseUseCase : ICreateWarehouseUseCase
{
    private readonly IWarehouseRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateWarehouseUseCase> _logger;

    public CreateWarehouseUseCase(IWarehouseRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateWarehouseUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<WarehouseResponseDto> ExecuteAsync(CreateWarehouseRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Mã kho không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên kho không được để trống.");

        var code = request.Code.Trim();
        if (await _repo.CodeExistsAsync(code, ct: ct))
            throw new DomainException($"Kho '{code}' đã tồn tại.");

        var warehouse = new WarehouseEntity { Code = code, Name = request.Name.Trim(), IsActive = request.IsActive };
        var created   = await _repo.AddAsync(warehouse, ct);
        _logger.LogInformation("Created warehouse {Id} '{Code}'", created.Id, created.Code);

        var dto = GetWarehousesUseCase.MapToDto(created);
        await _broadcaster.WarehouseCreatedAsync(dto, ct);
        return dto;
    }
}
