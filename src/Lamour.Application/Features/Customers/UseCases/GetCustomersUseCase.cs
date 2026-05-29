using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class GetCustomersUseCase : IGetCustomersUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<GetCustomersUseCase> _logger;

    public GetCustomersUseCase(ICustomerRepository repo, ILogger<GetCustomersUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all customers");
        var customers = await _repo.GetAllAsync(ct);
        return customers.Select(MapToDto);
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
