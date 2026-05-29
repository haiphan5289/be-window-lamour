using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class CreateCustomerUseCase : ICreateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<CreateCustomerUseCase> _logger;

    public CreateCustomerUseCase(ICustomerRepository repo, ILogger<CreateCustomerUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<CustomerResponseDto> ExecuteAsync(CreateCustomerRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Customer name is required.");

        var code = await _repo.GetNextCodeAsync(ct);

        var customer = new Customer
        {
            Code          = code,
            Name          = request.Name.Trim(),
            Address       = request.Address,
            Province      = request.Province,
            CustomerGroup = request.CustomerGroup,
            TaxCode       = request.TaxCode,
            Phone         = request.Phone,
            SaleCare      = request.SaleCare,
        };

        var created = await _repo.AddAsync(customer, ct);
        _logger.LogInformation("Created customer {Id} with code {Code}", created.Id, created.Code);

        return MapToDto(created);
    }

    private static CustomerResponseDto MapToDto(Customer c) => new()
    {
        Id            = c.Id,
        Code          = c.Code,
        Name          = c.Name,
        Address       = c.Address,
        Province      = c.Province,
        CustomerGroup = c.CustomerGroup,
        TaxCode       = c.TaxCode,
        Phone         = c.Phone,
        SaleCare      = c.SaleCare,
    };
}
