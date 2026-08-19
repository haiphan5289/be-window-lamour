using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.Employees.UseCases;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.UseCases;

public class ImportExcelEmployeesUseCase : IImportExcelEmployeesUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<ImportExcelEmployeesUseCase> _logger;

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tên nhân viên"]  = "name",
        ["tên nv"]         = "name",
        ["giới tính"]      = "gender",
        ["điện thoại"]     = "phone",
        ["sđt"]            = "phone",
        ["vai trò"]        = "role",
        ["role"]           = "role",
        ["đơn vị"]         = "unit",
        ["chức danh"]      = "job_title",
        ["số tài khoản"]   = "bank_account_number",
        ["ngân hàng"]      = "bank_name",
    };

    public ImportExcelEmployeesUseCase(
        IEmployeeRepository repo,
        INotificationBroadcaster broadcaster,
        ILogger<ImportExcelEmployeesUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ImportEmployeeResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default)
    {
        var errors         = new List<ImportRowErrorDto>();
        var validEmployees = new List<Employee>();

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheet(1);

        var colMap   = BuildColumnMap(ws.Row(1));
        var dataRows = ws.RowsUsed().Skip(1).ToList();

        var nextCodeStr = await _repo.GetNextCodeAsync(ct);
        int codeCounter = int.Parse(nextCodeStr[2..]);

        foreach (var row in dataRows)
        {
            int rowNum = row.RowNumber();
            var name   = GetCell(row, colMap, "name");
            var phone  = GetCell(row, colMap, "phone");

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = "Tên nhân viên không được để trống." });
                continue;
            }

            var roleStr = GetCell(row, colMap, "role");
            if (string.IsNullOrWhiteSpace(roleStr)) roleStr = "Cashier";
            if (!Enum.TryParse<EmployeeRole>(roleStr, ignoreCase: true, out var role))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Vai trò '{roleStr}' không hợp lệ (Admin/Cashier/Warehouse)." });
                continue;
            }

            var genderStr = GetCell(row, colMap, "gender");
            if (string.IsNullOrWhiteSpace(genderStr)) genderStr = "Nam";
            var gender = EmployeeGenders.AllowedValues.FirstOrDefault(v => v.Equals(genderStr, StringComparison.OrdinalIgnoreCase));
            if (gender is null)
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Giới tính '{genderStr}' không hợp lệ ({string.Join("/", EmployeeGenders.AllowedValues)})." });
                continue;
            }

            var unitStr = GetCell(row, colMap, "unit");
            if (string.IsNullOrWhiteSpace(unitStr)) unitStr = "Tiệm spa";
            var unit = EmployeeUnits.AllowedValues.FirstOrDefault(v => v.Equals(unitStr, StringComparison.OrdinalIgnoreCase));
            if (unit is null)
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Đơn vị '{unitStr}' không hợp lệ ({string.Join("/", EmployeeUnits.AllowedValues)})." });
                continue;
            }

            var jobTitleStr = GetCell(row, colMap, "job_title");
            if (string.IsNullOrWhiteSpace(jobTitleStr)) jobTitleStr = "Khac";
            if (!Enum.TryParse<EmployeeJobTitle>(jobTitleStr, ignoreCase: true, out var jobTitle))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Chức danh '{jobTitleStr}' không hợp lệ (Admin/TruongPhong/NhanVienBanHang/NhanVienKho/ThuNgan/Khac)." });
                continue;
            }

            var bankAccountNumber = GetCell(row, colMap, "bank_account_number");
            var bankName          = GetCell(row, colMap, "bank_name");
            var code              = $"NV{codeCounter++:D5}";

            validEmployees.Add(new Employee
            {
                Code              = code,
                Name              = name,
                Gender            = gender,
                Phone             = phone,
                Role              = role,
                Unit              = unit,
                JobTitle          = jobTitle,
                BankAccountNumber = string.IsNullOrWhiteSpace(bankAccountNumber) ? null : bankAccountNumber,
                BankName          = string.IsNullOrWhiteSpace(bankName) ? null : bankName,
                // Không có cột mật khẩu trong file import — mặc định = SĐT (2026-08-19: giờ optional,
                // nếu cũng trống thì fallback về Code), giống hành vi CreateEmployeeUseCase.
                PasswordHash      = HashPassword(!string.IsNullOrWhiteSpace(phone) ? phone : code),
                IsActive          = true,
            });
        }

        if (validEmployees.Count > 0)
        {
            await _repo.AddRangeAsync(validEmployees, ct);
            _logger.LogInformation("Imported {Count}/{Total} employees from Excel", validEmployees.Count, dataRows.Count);
            await _broadcaster.EmployeesBulkChangedAsync(ct);
        }

        return new ImportEmployeeResultDto
        {
            Total    = dataRows.Count,
            Imported = validEmployees.Count,
            Skipped  = errors.Count,
            Errors   = errors,
        };
    }

    private static string HashPassword(string password)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.Value.ToString().Trim();
            if (HeaderAliases.TryGetValue(header, out var key))
                map[key] = cell.Address.ColumnNumber;
        }
        return map;
    }

    private static string GetCell(IXLRow row, Dictionary<string, int> colMap, string key)
    {
        if (!colMap.TryGetValue(key, out var colNum)) return string.Empty;
        return row.Cell(colNum).Value.ToString().Trim();
    }
}
