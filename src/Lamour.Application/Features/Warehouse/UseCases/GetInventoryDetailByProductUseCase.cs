using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Warehouse.Dtos;
using Lamour.Application.Features.Warehouse.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouse.UseCases;

// Drill-down từ Tổng hợp tồn kho — "Sổ chi tiết vật tư hàng hóa" cho 1 sản phẩm: liệt kê từng
// giao dịch Nhập/Xuất/Trả lại trong khoảng ngày, kèm Tồn chạy dần (running balance) sau mỗi dòng.
public class GetInventoryDetailByProductUseCase : IGetInventoryDetailByProductUseCase
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IProductRepository   _productRepo;
    private readonly ILogger<GetInventoryDetailByProductUseCase> _logger;

    public GetInventoryDetailByProductUseCase(
        IInventoryRepository inventoryRepo,
        IProductRepository   productRepo,
        ILogger<GetInventoryDetailByProductUseCase> logger)
    {
        _inventoryRepo = inventoryRepo;
        _productRepo   = productRepo;
        _logger        = logger;
    }

    public async Task<InventoryDetailResponseDto?> ExecuteAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product is null) return null;

        _logger.LogInformation("Fetching inventory detail for product {ProductId} from {From} to {To}",
            productId, fromDate, toDate);

        // Opening = Closing(hiện tại) − Import(range) + Export(range) — khớp công thức đã dùng ở
        // GetInventorySummaryUseCase (ClosingQty là tồn thực tế NGAY BÂY GIỜ từ ProductWarehouseStock,
        // không phải tồn tại thời điểm toDate — chấp nhận giới hạn này giống màn Tổng hợp).
        var imports      = await _inventoryRepo.GetImportsByProductAsync(fromDate, toDate, warehouseIds, ct);
        var exportQtys    = await _inventoryRepo.GetExportQtyByProductAsync(fromDate, toDate, warehouseIds, ct);
        var closingQtys   = await _inventoryRepo.GetClosingQtyByProductAsync(warehouseIds, ct);

        imports.TryGetValue(productId, out var imp);
        exportQtys.TryGetValue(productId, out var exportQty);
        closingQtys.TryGetValue(productId, out var closingQty);

        var importQtyTotal = imp.Qty;
        var openingQty      = closingQty - importQtyTotal + exportQty;
        var openingValue    = openingQty * product.CostPrice;
        var closingValue    = closingQty * product.CostPrice;

        var rawLines = await _inventoryRepo.GetTransactionLinesByProductAsync(productId, fromDate, toDate, warehouseIds, ct);

        var lines = new List<InventoryDetailLineDto>();
        var runningQty   = openingQty;
        var runningValue = openingValue;

        foreach (var l in rawLines.OrderBy(l => l.AccountingDate).ThenBy(l => l.DocumentNumber))
        {
            // Xuất/Trả lại định giá theo CostPrice HIỆN TẠI của sản phẩm — khớp cách ExportValue
            // được tính ở GetInventorySummaryUseCase (không dùng UnitPrice bán ra trên chứng từ).
            var exportValue = l.ExportQty * product.CostPrice;
            var importValue = l.DocumentType == "SalesReturn" ? l.ImportQty * product.CostPrice : l.ImportValue;

            runningQty   += l.ImportQty - l.ExportQty;
            runningValue =  runningQty * product.CostPrice;

            lines.Add(new InventoryDetailLineDto
            {
                AccountingDate = l.AccountingDate,
                DocumentDate   = l.DocumentDate,
                DocumentNumber = l.DocumentNumber,
                DocumentType   = l.DocumentType,
                SourceId       = l.SourceId,
                Description    = l.Description,
                Unit           = l.Unit,
                ImportQty      = l.ImportQty,
                ImportValue    = importValue,
                ExportQty      = l.ExportQty,
                ExportValue    = exportValue,
                RunningQty     = runningQty,
                RunningValue   = runningValue,
            });
        }

        return new InventoryDetailResponseDto
        {
            ProductId    = product.Id,
            Code         = product.Code,
            Name         = product.Name,
            Unit         = product.Unit,
            OpeningQty   = openingQty,
            OpeningValue = openingValue,
            ClosingQty   = closingQty,
            ClosingValue = closingValue,
            Lines        = lines,
        };
    }
}
