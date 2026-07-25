using Lamour.Application.Abstractions;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class DuplicateCustomerUseCase : IDuplicateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DuplicateCustomerUseCase> _logger;

    public DuplicateCustomerUseCase(ICustomerRepository repo, INotificationBroadcaster broadcaster, ILogger<DuplicateCustomerUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<CustomerResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Customer {id} not found.");

        var newCode = await _repo.GetNextCodeAsync(ct);

        var copy = new Customer
        {
            Code          = newCode,
            Name          = source.Name,
            Address       = source.Address,
            Province      = source.Province,
            CustomerGroup = source.CustomerGroup,
            TaxCode       = source.TaxCode,
            Phone         = source.Phone,
        };

        var created = await _repo.AddAsync(copy, ct);
        _logger.LogInformation("Duplicated customer {SourceId} → {NewId}", id, created.Id);

        var dto = new CustomerResponseDto
        {
            Id            = created.Id,
            Code          = created.Code,
            Name          = created.Name,
            Address       = created.Address,
            Province      = created.Province,
            CustomerGroup = created.CustomerGroup,
            TaxCode       = created.TaxCode,
            Phone         = created.Phone,
        };

        await _broadcaster.CustomerCreatedAsync(dto, ct);
        return dto;
    }
}
