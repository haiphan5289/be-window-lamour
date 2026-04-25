using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class UpdateEmployeeUseCase : IUpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly ILogger<UpdateEmployeeUseCase> _logger;

    public UpdateEmployeeUseCase(IEmployeeRepository repo, ILogger<UpdateEmployeeUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<EmployeeResponseDto> ExecuteAsync(int id, UpdateEmployeeRequestDto request, CancellationToken ct = default)
    {
        var employee = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Nhân viên {id} không tồn tại.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên nhân viên không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new DomainException("Số điện thoại không được để trống.");

        if (!Enum.TryParse<EmployeeRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException($"Role '{request.Role}' không hợp lệ. Giá trị hợp lệ: Admin, Cashier, Warehouse.");

        employee.Name     = request.Name.Trim();
        employee.Phone    = request.Phone.Trim();
        employee.Role     = role;
        employee.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
            employee.PasswordHash = CreateEmployeeUseCase.HashPassword(request.Password);

        var updated = await _repo.UpdateAsync(employee, ct);
        _logger.LogInformation("Updated employee {Id}", id);

        return GetEmployeesUseCase.MapToDto(updated);
    }
}
