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

        // Tính PER KHO: Opening(kho) = Closing(kho, hiện tại) − Nhập(kho, range) + Xuất(kho, range) — khớp công thức
        // GetInventorySummaryUseCase (ClosingQty là tồn thực tế NGAY BÂY GIỜ từ ProductWarehouseStock, không phải
        // tồn tại thời điểm toDate). Nhập/Xuất ròng của kho = Σ(ImportQty − ExportQty) trên các dòng của kho
        // (Hàng bán bị trả lại tính vào ImportQty), nên không cần query tổng riêng.
        var rawLines = (await _inventoryRepo.GetTransactionLinesByProductAsync(productId, fromDate, toDate, warehouseIds, ct)).ToList();
        var closingByWarehouse = await _inventoryRepo.GetClosingQtyByWarehouseProductAsync(warehouseIds, productId, ct);
        var warehouseList      = await _inventoryRepo.GetWarehousesAsync(warehouseIds, ct);

        var info = new Dictionary<int, (string Code, string Name)>();
        foreach (var w in warehouseList) info[w.Id] = (w.Code, w.Name);
        foreach (var l in rawLines) info.TryAdd(l.WarehouseId, (l.WarehouseCode, l.WarehouseName));

        var netByWarehouse = rawLines
            .GroupBy(l => l.WarehouseId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.ImportQty - l.ExportQty));

        // Kho liên quan: đúng các kho được chọn; nếu không chọn kho nào thì mọi kho có tồn hoặc có giao dịch.
        var involvedIds = warehouseIds is { Count: > 0 }
            ? warehouseList.Select(w => w.Id).ToHashSet()
            : closingByWarehouse.Keys.Select(k => k.WarehouseId).Concat(netByWarehouse.Keys).ToHashSet();

        var warehouses = new List<InventoryDetailWarehouseDto>();
        var openingByWarehouse = new Dictionary<int, int>();
        foreach (var id in involvedIds.Where(info.ContainsKey).OrderBy(id => info[id].Name, StringComparer.OrdinalIgnoreCase))
        {
            closingByWarehouse.TryGetValue((id, productId), out var closing);
            netByWarehouse.TryGetValue(id, out var net);
            var opening = closing - net;
            openingByWarehouse[id] = opening;

            warehouses.Add(new InventoryDetailWarehouseDto
            {
                WarehouseId   = id,
                WarehouseCode = info[id].Code,
                WarehouseName = info[id].Name,
                OpeningQty    = opening,
                OpeningValue  = opening * product.CostPrice,
                ClosingQty    = closing,
                ClosingValue  = closing * product.CostPrice,
            });
        }

        var warehouseOrder = warehouses.Select((w, i) => (w.WarehouseId, Index: i)).ToDictionary(x => x.WarehouseId, x => x.Index);
        var lines = new List<InventoryDetailLineDto>();
        var runningByWarehouse = new Dictionary<int, int>(openingByWarehouse);

        foreach (var l in rawLines
                     .Where(l => warehouseOrder.ContainsKey(l.WarehouseId))
                     .OrderBy(l => warehouseOrder[l.WarehouseId])
                     .ThenBy(l => l.AccountingDate)
                     .ThenBy(l => l.DocumentNumber))
        {
            // Xuất/Trả lại định giá theo CostPrice HIỆN TẠI của sản phẩm — khớp cách ExportValue
            // được tính ở GetInventorySummaryUseCase (không dùng UnitPrice bán ra trên chứng từ).
            var exportValue = l.ExportQty * product.CostPrice;
            var importValue = l.DocumentType == "SalesReturn" ? l.ImportQty * product.CostPrice : l.ImportValue;

            var qty       = l.ImportQty + l.ExportQty;
            var lineValue = l.ImportQty > 0 ? importValue : exportValue;
            var unitPrice = qty > 0 ? lineValue / qty : 0m;

            var runningQty = runningByWarehouse[l.WarehouseId] + l.ImportQty - l.ExportQty;
            runningByWarehouse[l.WarehouseId] = runningQty;

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
                WarehouseId    = l.WarehouseId,
                WarehouseCode  = info[l.WarehouseId].Code,
                WarehouseName  = info[l.WarehouseId].Name,
                UnitPrice      = unitPrice,
                RunningQty     = runningQty,
                RunningValue   = runningQty * product.CostPrice,
            });
        }

        var openingQty = warehouses.Sum(w => w.OpeningQty);
        var closingQty = warehouses.Sum(w => w.ClosingQty);

        return new InventoryDetailResponseDto
        {
            ProductId    = product.Id,
            Code         = product.Code,
            Name         = product.Name,
            Unit         = product.Unit,
            OpeningQty   = openingQty,
            OpeningValue = openingQty * product.CostPrice,
            ClosingQty   = closingQty,
            ClosingValue = closingQty * product.CostPrice,
            Warehouses   = warehouses,
            Lines        = lines,
        };
    }
}
