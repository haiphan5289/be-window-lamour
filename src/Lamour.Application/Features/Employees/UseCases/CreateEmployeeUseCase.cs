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

        if (!Enum.TryParse<EmployeeRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException($"Role '{request.Role}' không hợp lệ. Giá trị hợp lệ: Admin, Cashier, Warehouse.");

        var unit = EmployeeUnits.AllowedValues.FirstOrDefault(v => v.Equals(request.Unit, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Đơn vị '{request.Unit}' không hợp lệ. Giá trị hợp lệ: {string.Join(", ", EmployeeUnits.AllowedValues)}.");

        var gender = EmployeeGenders.AllowedValues.FirstOrDefault(v => v.Equals(request.Gender, StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException($"Giới tính '{request.Gender}' không hợp lệ. Giá trị hợp lệ: {string.Join(", ", EmployeeGenders.AllowedValues)}.");

        if (!Enum.TryParse<EmployeeJobTitle>(request.JobTitle, ignoreCase: true, out var jobTitle))
            throw new DomainException($"Chức danh '{request.JobTitle}' không hợp lệ. Giá trị hợp lệ: Admin, TruongPhong, NhanVienBanHang, NhanVienKho, ThuNgan, Khac.");

        var code = await _repo.GetNextCodeAsync(ct);

        // Số điện thoại giờ optional (2026-08-19) — mật khẩu mặc định khi bỏ trống: ưu tiên Phone,
        // nếu Phone cũng trống thì dùng Code (luôn có giá trị) để không bao giờ hash chuỗi rỗng.
        var phone = request.Phone.Trim();
        var rawPassword = !string.IsNullOrWhiteSpace(request.Password) ? request.Password
            : !string.IsNullOrWhiteSpace(phone) ? phone
            : code;

        var employee = new Employee
        {
            Code              = code,
            Name              = request.Name.Trim(),
            Gender            = gender,
            Phone             = phone,
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
