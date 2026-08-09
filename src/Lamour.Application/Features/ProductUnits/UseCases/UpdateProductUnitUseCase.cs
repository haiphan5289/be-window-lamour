using Lamour.Application.Abstractions;
using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public class UpdateProductUnitUseCase : IUpdateProductUnitUseCase
{
    private readonly IProductUnitRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateProductUnitUseCase> _logger;

    public UpdateProductUnitUseCase(IProductUnitRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateProductUnitUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ProductUnitResponseDto> ExecuteAsync(int id, UpdateProductUnitRequestDto request, CancellationToken ct = default)
    {
        var unit = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product unit {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên đơn vị tính không được để trống.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, excludeId: id, ct: ct))
            throw new DomainException($"Đơn vị tính '{name}' đã tồn tại.");

        unit.Name = name;
        var updated = await _repo.UpdateAsync(unit, ct);
        _logger.LogInformation("Updated product unit {Id}", id);

        var dto = GetProductUnitsUseCase.MapToDto(updated);
        await _broadcaster.ProductUnitUpdatedAsync(dto, ct);
        return dto;
    }
}
