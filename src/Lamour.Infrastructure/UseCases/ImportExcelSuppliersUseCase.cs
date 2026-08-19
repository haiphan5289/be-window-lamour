using ClosedXML.Excel;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Application.Features.Suppliers.UseCases;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.UseCases;

public class ImportExcelSuppliersUseCase : IImportExcelSuppliersUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<ImportExcelSuppliersUseCase> _logger;

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mã ncc"]              = "code",
        ["mã nhà cung cấp"]     = "code",
        ["mã"]                  = "code",
        ["tên ncc"]             = "name",
        ["tên nhà cung cấp"]    = "name",
        ["địa chỉ"]             = "address",
        ["nhóm"]                = "group",
        ["nhóm kh, ncc"]        = "group",
        ["nhóm kh ncc"]         = "group",
        ["mã số thuế"]          = "tax_code",
        ["mst"]                 = "tax_code",
        ["điện thoại"]          = "phone",
        ["sđt"]                 = "phone",
    };

    public ImportExcelSuppliersUseCase(
        ISupplierRepository repo,
        INotificationBroadcaster broadcaster,
        ILogger<ImportExcelSuppliersUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ImportSupplierResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default)
    {
        var errors         = new List<ImportRowErrorDto>();
        var validSuppliers = new List<Supplier>();
        var seenCodes      = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheet(1);

        var colMap   = BuildColumnMap(ws.Row(1));
        var dataRows = ws.RowsUsed().Skip(1).ToList();

        foreach (var row in dataRows)
        {
            int rowNum = row.RowNumber();
            var code   = GetCell(row, colMap, "code");
            var name   = GetCell(row, colMap, "name");

            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = "Mã nhà cung cấp không được để trống." });
                continue;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = "Tên nhà cung cấp không được để trống." });
                continue;
            }
            if (!seenCodes.Add(code) || await _repo.CodeExistsAsync(code, ct: ct))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Mã nhà cung cấp '{code}' đã tồn tại." });
                continue;
            }

            validSuppliers.Add(new Supplier
            {
                Code    = code,
                Name    = name,
                Address = GetCell(row, colMap, "address"),
                Group   = GetCell(row, colMap, "group"),
                TaxCode = GetCell(row, colMap, "tax_code"),
                Phone   = GetCell(row, colMap, "phone"),
            });
        }

        if (validSuppliers.Count > 0)
        {
            await _repo.AddRangeAsync(validSuppliers, ct);
            _logger.LogInformation("Imported {Count}/{Total} suppliers from Excel", validSuppliers.Count, dataRows.Count);
            await _broadcaster.SuppliersBulkChangedAsync(ct);
        }

        return new ImportSupplierResultDto
        {
            Total    = dataRows.Count,
            Imported = validSuppliers.Count,
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
