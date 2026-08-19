using Lamour.Application.Abstractions;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class UpdateEmployeeUseCase : IUpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateEmployeeUseCase> _logger;

    public UpdateEmployeeUseCase(IEmployeeRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateEmployeeUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<EmployeeResponseDto> ExecuteAsync(int id, UpdateEmployeeRequestDto request, CancellationToken ct = default)
    {
        var employee = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Nhân viên {id} không tồn tại.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên nhân viên không được để trống.");

        if (!Enum.TryParse<EmployeeRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException($"Role '{request.Role}' không hợp lệ. Giá trị hợp lệ: Admin, Cashier, Warehouse.");

        var unit = EmployeeUnits.AllowedValues.FirstOrDefault(v => v.Equals(request.Unit, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Đơn vị '{request.Unit}' không hợp lệ. Giá trị hợp lệ: {string.Join(", ", EmployeeUnits.AllowedValues)}.");

        var gender = EmployeeGenders.AllowedValues.FirstOrDefault(v => v.Equals(request.Gender, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Giới tính '{request.Gender}' không hợp lệ. Giá trị hợp lệ: {string.Join(", ", EmployeeGenders.AllowedValues)}.");

        if (!Enum.TryParse<EmployeeJobTitle>(request.JobTitle, ignoreCase: true, out var jobTitle))
            throw new DomainException($"Chức danh '{request.JobTitle}' không hợp lệ. Giá trị hợp lệ: Admin, TruongPhong, NhanVienBanHang, NhanVienKho, ThuNgan, Khac.");

        employee.Name              = request.Name.Trim();
        employee.Gender            = gender;
        employee.Phone             = request.Phone.Trim();
        employee.Role              = role;
        employee.Unit              = unit;
        employee.JobTitle          = jobTitle;
        employee.BankAccountNumber = string.IsNullOrWhiteSpace(request.BankAccountNumber) ? null : request.BankAccountNumber.Trim();
        employee.BankName          = string.IsNullOrWhiteSpace(request.BankName) ? null : request.BankName.Trim();
        employee.IsActive          = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
            employee.PasswordHash = CreateEmployeeUseCase.HashPassword(request.Password);

        var updated = await _repo.UpdateAsync(employee, ct);
        _logger.LogInformation("Updated employee {Id}", id);

        var dto = GetEmployeesUseCase.MapToDto(updated);
        await _broadcaster.EmployeeUpdatedAsync(dto, ct);
        return dto;
    }
}
