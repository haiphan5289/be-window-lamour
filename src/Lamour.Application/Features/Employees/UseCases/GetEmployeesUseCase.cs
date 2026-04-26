using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class GetEmployeesUseCase : IGetEmployeesUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly ILogger<GetEmployeesUseCase> _logger;

    public GetEmployeesUseCase(IEmployeeRepository repo, ILogger<GetEmployeesUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all employees");
        var employees = await _repo.GetAllAsync(ct);
        return employees.Select(MapToDto);
    }

    internal static EmployeeResponseDto MapToDto(Employee e) => new()
    {
        Id                = e.Id,
        Code              = e.Code,
        Name              = e.Name,
        Phone             = e.Phone,
        Role              = e.Role.ToString(),
        Unit              = e.Unit.ToString(),
        JobTitle          = e.JobTitle.ToString(),
        BankAccountNumber = e.BankAccountNumber,
        BankName          = e.BankName,
        IsActive          = e.IsActive,
    };
}
