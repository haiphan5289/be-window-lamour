using ClosedXML.Excel;
using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Products.UseCases;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.UseCases;

// Import chỉ hỗ trợ bộ field "cốt lõi" khớp với các cột hiển thị trên ProductListView (WPF) —
// Mã, Tên, Danh mục, Đơn vị, Giá nhập, Giá bán, Tồn kho, Hoạt động. Product có ~30 field khác
// (thuế, tài khoản kế toán, kho ngầm định...) nhưng import hàng loạt cho từng đó FK/enum phức tạp
// hơn nhiều so với Customer/Supplier/Employee — để ngoài phạm vi, sửa qua form từng sản phẩm.
public class ImportExcelProductsUseCase : IImportExcelProductsUseCase
{
    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<ImportExcelProductsUseCase> _logger;

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mã sản phẩm"]  = "code",
        ["mã hàng"]      = "code",
        ["mã"]           = "code",
        ["tên sản phẩm"] = "name",
        ["tên hàng"]     = "name",
        ["danh mục"]     = "category",
        ["đơn vị"]       = "unit",
        ["đvt"]          = "unit",
        ["giá nhập"]     = "cost_price",
        ["giá bán"]      = "selling_price",
        ["tồn kho"]      = "stock_quantity",
    };

    public ImportExcelProductsUseCase(
        IProductRepository repo,
        ICategoryRepository categoryRepo,
        INotificationBroadcaster broadcaster,
        ILogger<ImportExcelProductsUseCase> logger)
    {
        _repo         = repo;
        _categoryRepo = categoryRepo;
        _broadcaster  = broadcaster;
        _logger       = logger;
    }

    public async Task<ImportProductResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default)
    {
        var errors        = new List<ImportRowErrorDto>();
        var validProducts = new List<Product>();
        var seenCodes     = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheet(1);

        var colMap   = BuildColumnMap(ws.Row(1));
        var dataRows = ws.RowsUsed().Skip(1).ToList();

        var categories       = await _categoryRepo.GetAllAsync(ct);
        var categoriesByName = categories
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        foreach (var row in dataRows)
        {
            int rowNum = row.RowNumber();
            var name   = GetCell(row, colMap, "name");

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = "Tên sản phẩm không được để trống." });
                continue;
            }

            var code = GetCell(row, colMap, "code");
            if (!string.IsNullOrWhiteSpace(code))
            {
                if (!seenCodes.Add(code) || await _repo.CodeExistsAsync(code, ct: ct))
                {
                    errors.Add(new ImportRowErrorDto { Row = rowNum, Reason = $"Mã sản phẩm '{code}' đã tồn tại." });
                    continue;
                }
            }

            var categoryName = GetCell(row, colMap, "category");
            int? categoryId = !string.IsNullOrWhiteSpace(categoryName) && categoriesByName.TryGetValue(categoryName, out var catId)
                ? catId
                : null;

            var costPrice     = ParseDecimal(GetCell(row, colMap, "cost_price"));
            var sellingPrice  = ParseDecimal(GetCell(row, colMap, "selling_price"));
            var stockQuantity = ParseInt(GetCell(row, colMap, "stock_quantity"));

            validProducts.Add(new Product
            {
                Code          = code,
                Name          = name,
                CategoryId    = categoryId,
                Unit          = GetCell(row, colMap, "unit"),
                CostPrice     = costPrice,
                SellingPrice  = sellingPrice,
                StockQuantity = stockQuantity,
                IsActive      = true,
            });
        }

        if (validProducts.Count > 0)
        {
            await _repo.AddRangeAsync(validProducts, ct);
            _logger.LogInformation("Imported {Count}/{Total} products from Excel", validProducts.Count, dataRows.Count);
            await _broadcaster.ProductsBulkChangedAsync(ct);
        }

        return new ImportProductResultDto
        {
            Total    = dataRows.Count,
            Imported = validProducts.Count,
            Skipped  = errors.Count,
            Errors   = errors,
        };
    }

    private static decimal ParseDecimal(string value) =>
        decimal.TryParse(value, out var result) ? result : 0m;

    private static int ParseInt(string value) =>
        int.TryParse(value, out var result) ? result : 0;

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
