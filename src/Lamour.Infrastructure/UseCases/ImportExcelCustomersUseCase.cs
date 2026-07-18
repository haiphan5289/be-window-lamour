using ClosedXML.Excel;
using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Customers.UseCases;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.UseCases;

public class ImportExcelCustomersUseCase : IImportExcelCustomersUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILogger<ImportExcelCustomersUseCase> _logger;

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tên khách hàng"] = "name",
        ["tên kh"]         = "name",
        ["địa chỉ"]        = "address",
        ["tỉnh/tp"]        = "province",
        ["tỉnh/thành phố"] = "province",
        ["tỉnh"]           = "province",
        ["nhóm kh/ncc"]    = "customer_group",
        ["nhóm kh ncc"]    = "customer_group",
        ["nhóm kh"]        = "customer_group",
        ["mã số thuế"]     = "tax_code",
        ["mst"]            = "tax_code",
        ["điện thoại"]     = "phone",
        ["sđt"]            = "phone",
        ["tên nhân viên"]  = "sale_care",
        ["nhân viên"]      = "sale_care",
    };

    public ImportExcelCustomersUseCase(ICustomerRepository repo, IEmployeeRepository employeeRepo, ILogger<ImportExcelCustomersUseCase> logger)
    {
        _repo         = repo;
        _employeeRepo = employeeRepo;
        _logger       = logger;
    }

    public async Task<ImportCustomerResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default)
    {
        var errors         = new List<ImportRowErrorDto>();
        var validCustomers = new List<Customer>();

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheet(1);

        var colMap   = BuildColumnMap(ws.Row(1));
        var dataRows = ws.RowsUsed().Skip(1).ToList();

        var nextCodeStr = await _repo.GetNextCodeAsync(ct);
        int codeCounter = int.Parse(nextCodeStr[2..]);

        var employees        = await _employeeRepo.GetAllAsync(ct);
        var employeesByName  = employees
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        foreach (var row in dataRows)
        {
            int rowNum = row.RowNumber();
            var name   = GetCell(row, colMap, "name");

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = "Tên khách hàng không được để trống." });
                continue;
            }

            var saleCareName = GetCell(row, colMap, "sale_care");
            int? saleCareEmployeeId = !string.IsNullOrWhiteSpace(saleCareName) && employeesByName.TryGetValue(saleCareName, out var employeeId)
                ? employeeId
                : null;

            validCustomers.Add(new Customer
            {
                Code               = $"KH{codeCounter++:D5}",
                Name               = name,
                Address            = GetCell(row, colMap, "address"),
                Province           = GetCell(row, colMap, "province"),
                CustomerGroup      = GetCell(row, colMap, "customer_group"),
                TaxCode            = GetCell(row, colMap, "tax_code"),
                Phone              = GetCell(row, colMap, "phone"),
                SaleCareEmployeeId = saleCareEmployeeId,
            });
        }

        if (validCustomers.Count > 0)
        {
            await _repo.AddRangeAsync(validCustomers, ct);
            _logger.LogInformation("Imported {Count}/{Total} customers from Excel", validCustomers.Count, dataRows.Count);
        }

        return new ImportCustomerResultDto
        {
            Total    = dataRows.Count,
            Imported = validCustomers.Count,
            Skipped  = errors.Count,
            Errors   = errors,
        };
    }

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
