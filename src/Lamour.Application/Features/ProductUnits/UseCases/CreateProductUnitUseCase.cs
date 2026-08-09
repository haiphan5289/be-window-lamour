using Lamour.Application.Abstractions;
using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public class CreateProductUnitUseCase : ICreateProductUnitUseCase
{
    private readonly IProductUnitRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateProductUnitUseCase> _logger;

    public CreateProductUnitUseCase(IProductUnitRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateProductUnitUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ProductUnitResponseDto> ExecuteAsync(CreateProductUnitRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên đơn vị tính không được để trống.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, ct: ct))
            throw new DomainException($"Đơn vị tính '{name}' đã tồn tại.");

        var unit    = new ProductUnit { Name = name };
        var created = await _repo.AddAsync(unit, ct);
        _logger.LogInformation("Created product unit {Id} '{Name}'", created.Id, created.Name);

        var dto = GetProductUnitsUseCase.MapToDto(created);
        await _broadcaster.ProductUnitCreatedAsync(dto, ct);
        return dto;
    }
}
