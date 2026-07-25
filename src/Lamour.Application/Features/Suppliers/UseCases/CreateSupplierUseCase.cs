using Lamour.Application.Abstractions;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Suppliers.UseCases;

public class CreateSupplierUseCase : ICreateSupplierUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateSupplierUseCase> _logger;

    public CreateSupplierUseCase(ISupplierRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateSupplierUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<SupplierResponseDto> ExecuteAsync(CreateSupplierRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Supplier code is required.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Supplier name is required.");

        if (await _repo.CodeExistsAsync(request.Code, ct: ct))
            throw new DomainException($"Supplier with code '{request.Code}' already exists.");

        var supplier = new Supplier
        {
            Code           = request.Code.Trim(),
            Name           = request.Name.Trim(),
            Phone          = request.Phone,
            Address        = request.Address,
            Group          = request.Group,
            TaxCode        = request.TaxCode,
            IsStopTracking = request.IsStopTracking,
        };

        var created = await _repo.AddAsync(supplier, ct);
        _logger.LogInformation("Created supplier {Id} with code {Code}", created.Id, created.Code);

        var dto = MapToDto(created);
        await _broadcaster.SupplierCreatedAsync(dto, ct);
        return dto;
    }

    private static SupplierResponseDto MapToDto(Supplier s) => new()
    {
        Id             = s.Id,
        Code           = s.Code,
        Name           = s.Name,
        Address        = s.Address,
        Group          = s.Group,
        TaxCode        = s.TaxCode,
        Phone          = s.Phone,
        IsStopTracking = s.IsStopTracking,
    };
}
