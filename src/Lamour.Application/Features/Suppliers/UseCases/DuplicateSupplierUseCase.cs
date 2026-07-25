using Lamour.Application.Abstractions;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Suppliers.UseCases;

public class DuplicateSupplierUseCase : IDuplicateSupplierUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DuplicateSupplierUseCase> _logger;

    public DuplicateSupplierUseCase(ISupplierRepository repo, INotificationBroadcaster broadcaster, ILogger<DuplicateSupplierUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<SupplierResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Supplier {id} not found.");

        var newCode = source.Code + "_COPY";
        if (await _repo.CodeExistsAsync(newCode, ct: ct))
            throw new DomainException($"Supplier with code '{newCode}' already exists.");

        var copy = new Supplier
        {
            Code           = newCode,
            Name           = source.Name,
            Phone          = source.Phone,
            Address        = source.Address,
            Group          = source.Group,
            TaxCode        = source.TaxCode,
            IsStopTracking = source.IsStopTracking,
        };

        var created = await _repo.AddAsync(copy, ct);
        _logger.LogInformation("Duplicated supplier {SourceId} → {NewId}", id, created.Id);

        var dto = new SupplierResponseDto
        {
            Id             = created.Id,
            Code           = created.Code,
            Name           = created.Name,
            Address        = created.Address,
            Group          = created.Group,
            TaxCode        = created.TaxCode,
            Phone          = created.Phone,
            IsStopTracking = created.IsStopTracking,
        };

        await _broadcaster.SupplierCreatedAsync(dto, ct);
        return dto;
    }
}
