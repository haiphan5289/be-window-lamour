using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class DuplicateEmployeeUseCase : IDuplicateEmployeeUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly ILogger<DuplicateEmployeeUseCase> _logger;

    public DuplicateEmployeeUseCase(IEmployeeRepository repo, ILogger<DuplicateEmployeeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<EmployeeResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Nhân viên {id} không tồn tại.");

        var copy = new Employee
        {
            Name         = source.Name + " (Copy)",
            Phone        = source.Phone,
            Role         = source.Role,
            PasswordHash = CreateEmployeeUseCase.HashPassword(source.Phone),
            IsActive     = source.IsActive,
        };

        var created = await _repo.AddAsync(copy, ct);
        _logger.LogInformation("Duplicated employee {SourceId} → {NewId}", id, created.Id);

        return GetEmployeesUseCase.MapToDto(created);
    }
}
