using Lamour.Application.Abstractions;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class CreateCustomerUseCase : ICreateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateCustomerUseCase> _logger;

    public CreateCustomerUseCase(
        ICustomerRepository repo,
        IEmployeeRepository employeeRepo,
        INotificationBroadcaster broadcaster,
        ILogger<CreateCustomerUseCase> logger)
    {
        _repo         = repo;
        _employeeRepo = employeeRepo;
        _broadcaster  = broadcaster;
        _logger       = logger;
    }

    public async Task<CustomerResponseDto> ExecuteAsync(CreateCustomerRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Customer name is required.");

        Employee? saleCareEmployee = null;
        if (request.SaleCareEmployeeId.HasValue)
        {
            saleCareEmployee = await _employeeRepo.GetByIdAsync(request.SaleCareEmployeeId.Value, ct)
                ?? throw new DomainException($"Employee {request.SaleCareEmployeeId} not found.");
        }

        var code = await _repo.GetNextCodeAsync(ct);

        var customer = new Customer
        {
            Code              = code,
            Name              = request.Name.Trim(),
            Address           = request.Address,
            Province          = request.Province,
            CustomerGroup     = request.CustomerGroup,
            TaxCode           = request.TaxCode,
            Phone             = request.Phone,
            SaleCareEmployeeId = request.SaleCareEmployeeId,
        };

        var created = await _repo.AddAsync(customer, ct);
        _logger.LogInformation("Created customer {Id} with code {Code}", created.Id, created.Code);

        var dto = MapToDto(created, saleCareEmployee);
        await _broadcaster.CustomerCreatedAsync(dto, ct);
        return dto;
    }

    private static CustomerResponseDto MapToDto(Customer c, Employee? saleCareEmployee) => new()
    {
        Id                   = c.Id,
        Code                 = c.Code,
        Name                 = c.Name,
        Address              = c.Address,
        Province             = c.Province,
        CustomerGroup        = c.CustomerGroup,
        TaxCode              = c.TaxCode,
        Phone                = c.Phone,
        SaleCareEmployeeId   = c.SaleCareEmployeeId,
        SaleCareEmployeeName = saleCareEmployee?.Name,
    };
}
