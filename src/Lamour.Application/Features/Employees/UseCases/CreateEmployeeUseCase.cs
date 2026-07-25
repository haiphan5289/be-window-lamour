using System.Security.Cryptography;
using System.Text;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class CreateEmployeeUseCase : ICreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateEmployeeUseCase> _logger;

    public CreateEmployeeUseCase(IEmployeeRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateEmployeeUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<EmployeeResponseDto> ExecuteAsync(CreateEmployeeRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên nhân viên không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new DomainException("Số điện thoại không được để trống.");

        if (!Enum.TryParse<EmployeeRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException($"Role '{request.Role}' không hợp lệ. Giá trị hợp lệ: Admin, Cashier, Warehouse.");

        if (!Enum.TryParse<EmployeeUnit>(request.Unit, ignoreCase: true, out var unit))
            throw new DomainException($"Đơn vị '{request.Unit}' không hợp lệ. Giá trị hợp lệ: PGD, PKD, Spa, GD, Kho.");

        if (!Enum.TryParse<EmployeeJobTitle>(request.JobTitle, ignoreCase: true, out var jobTitle))
            throw new DomainException($"Chức danh '{request.JobTitle}' không hợp lệ. Giá trị hợp lệ: Admin, TruongPhong, NhanVienBanHang, NhanVienKho, ThuNgan, Khac.");

        var rawPassword = string.IsNullOrWhiteSpace(request.Password) ? request.Phone : request.Password;

        var code = await _repo.GetNextCodeAsync(ct);

        var employee = new Employee
        {
            Code              = code,
            Name              = request.Name.Trim(),
            Phone             = request.Phone.Trim(),
            Role              = role,
            Unit              = unit,
            JobTitle          = jobTitle,
            BankAccountNumber = string.IsNullOrWhiteSpace(request.BankAccountNumber) ? null : request.BankAccountNumber.Trim(),
            BankName          = string.IsNullOrWhiteSpace(request.BankName) ? null : request.BankName.Trim(),
            PasswordHash      = HashPassword(rawPassword),
            IsActive          = request.IsActive,
        };

        var created = await _repo.AddAsync(employee, ct);
        _logger.LogInformation("Created employee {Id} - {Name}", created.Id, created.Name);

        var dto = GetEmployeesUseCase.MapToDto(created);
        await _broadcaster.EmployeeCreatedAsync(dto, ct);
        return dto;
    }

    internal static string HashPassword(string password)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
