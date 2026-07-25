using Lamour.Application.Abstractions;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Suppliers.UseCases;

public class UpdateSupplierUseCase : IUpdateSupplierUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateSupplierUseCase> _logger;

    public UpdateSupplierUseCase(ISupplierRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateSupplierUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<SupplierResponseDto> ExecuteAsync(int id, UpdateSupplierRequestDto request, CancellationToken ct = default)
    {
        var supplier = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Supplier {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Supplier code is required.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Supplier name is required.");

        if (await _repo.CodeExistsAsync(request.Code, excludeId: id, ct: ct))
            throw new DomainException($"Supplier with code '{request.Code}' already exists.");

        supplier.Code           = request.Code.Trim();
        supplier.Name           = request.Name.Trim();
        supplier.Phone          = request.Phone;
        supplier.Address        = request.Address;
        supplier.Group          = request.Group;
        supplier.TaxCode        = request.TaxCode;
        supplier.IsStopTracking = request.IsStopTracking;

        var updated = await _repo.UpdateAsync(supplier, ct);
        _logger.LogInformation("Updated supplier {Id}", id);

        var dto = new SupplierResponseDto
        {
            Id             = updated.Id,
            Code           = updated.Code,
            Name           = updated.Name,
            Address        = updated.Address,
            Group          = updated.Group,
            TaxCode        = updated.TaxCode,
            Phone          = updated.Phone,
            IsStopTracking = updated.IsStopTracking,
        };

        await _broadcaster.SupplierUpdatedAsync(dto, ct);
        return dto;
    }
}
