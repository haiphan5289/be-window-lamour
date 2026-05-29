using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class UpdateCustomerUseCase : IUpdateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<UpdateCustomerUseCase> _logger;

    public UpdateCustomerUseCase(ICustomerRepository repo, ILogger<UpdateCustomerUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<CustomerResponseDto> ExecuteAsync(int id, UpdateCustomerRequestDto request, CancellationToken ct = default)
    {
        var customer = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Customer {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Customer name is required.");

        customer.Name          = request.Name.Trim();
        customer.Address       = request.Address;
        customer.Province      = request.Province;
        customer.CustomerGroup = request.CustomerGroup;
        customer.TaxCode       = request.TaxCode;
        customer.Phone         = request.Phone;
        customer.SaleCare      = request.SaleCare;

        var updated = await _repo.UpdateAsync(customer, ct);
        _logger.LogInformation("Updated customer {Id}", id);

        return new CustomerResponseDto
        {
            Id            = updated.Id,
            Code          = updated.Code,
            Name          = updated.Name,
            Address       = updated.Address,
            Province      = updated.Province,
            CustomerGroup = updated.CustomerGroup,
            TaxCode       = updated.TaxCode,
            Phone         = updated.Phone,
            SaleCare      = updated.SaleCare,
        };
    }
}
