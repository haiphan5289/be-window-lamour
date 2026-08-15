using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouse.UseCases;

// Gộp Nhập kho (WarehouseReceipt) + Xuất kho bán hàng (SalesOrder, giờ mang số XK) thành 1 danh
// sách giao dịch kho duy nhất, khớp UI tham chiếu MISA "Nhập, xuất kho". BE không có chứng từ
// "Xuất kho" riêng — mỗi SalesOrder đã ghi sổ CHÍNH LÀ 1 lần xuất kho (StockQuantity đã trừ lúc
// tạo/sửa đơn, xem CreateSalesOrderUseCase/UpdateSalesOrderUseCase), nên chỉ cần map lại, không
// tạo thêm bảng/entity mới.
public class GetWarehouseTransactionsUseCase : IGetWarehouseTransactionsUseCase
{
    private readonly IWarehouseReceiptRepository _receiptRepo;
    private readonly ISalesOrderRepository       _salesOrderRepo;
    private readonly IProductRepository          _productRepo;
    private readonly IWarehouseRepository        _warehouseRepo;
    private readonly ILogger<GetWarehouseTransactionsUseCase> _logger;

    public GetWarehouseTransactionsUseCase(
        IWarehouseReceiptRepository receiptRepo,
        ISalesOrderRepository salesOrderRepo,
        IProductRepository productRepo,
        IWarehouseRepository warehouseRepo,
        ILogger<GetWarehouseTransactionsUseCase> logger)
    {
        _receiptRepo    = receiptRepo;
        _salesOrderRepo = salesOrderRepo;
        _productRepo    = productRepo;
        _warehouseRepo  = warehouseRepo;
        _logger         = logger;
    }

    public async Task<IEnumerable<WarehouseTransactionResponseDto>> ExecuteAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default)
    {
        var result = new List<WarehouseTransactionResponseDto>();

        var wantImport = string.IsNullOrWhiteSpace(type) || type.Equals("import", StringComparison.OrdinalIgnoreCase);
        var wantExport = string.IsNullOrWhiteSpace(type) || type.Equals("export", StringComparison.OrdinalIgnoreCase);

        if (wantImport)
        {
            var receipts = await _receiptRepo.GetAllAsync(ct);
            result.AddRange(receipts
                .Where(r => InRange(r.AccountingDate, fromDate, toDate))
                .Select(MapReceipt));
        }

        if (wantExport)
        {
            var orders = await _salesOrderRepo.GetAllAsync(ct);
            var ordersInRange = orders.Where(o => InRange(o.AccountingDate, fromDate, toDate)).ToList();

            // SalesOrderLine không denormalize tên kho / TK Nợ-Có phía chi phí (632/1561) như
            // WarehouseReceiptLine — tra thêm Product (đã có sẵn CostAccount/StockAccount qua
            // IProductRepository.IncludeAll) và Warehouse (master list) 1 lần, dùng chung cho mọi dòng.
            var products   = (await _productRepo.GetAllAsync(ct)).ToDictionary(p => p.Id);
            var warehouses = (await _warehouseRepo.GetAllAsync(ct)).ToDictionary(w => w.Id);

            result.AddRange(ordersInRange.Select(o => MapSalesOrder(o, products, warehouses)));
        }

        return result.OrderByDescending(t => t.DocumentDate).ThenByDescending(t => t.Id);
    }

    private static bool InRange(DateTime date, DateTime? from, DateTime? to)
        => (!from.HasValue || date >= from.Value) && (!to.HasValue || date < to.Value.AddDays(1));

    private static WarehouseTransactionResponseDto MapReceipt(WarehouseReceipt r) => new()
    {
        Id                 = r.Id,
        TransactionType    = "Import",
        DocumentNumber     = r.ReceiptNumber,
        AccountingDate     = r.AccountingDate,
        DocumentDate       = r.DocumentDate,
        Description        = r.Description,
        TotalAmount        = r.TotalAmount,
        DeliveryOrReceiver = r.DeliveryPerson,
        ObjectName         = r.Customer?.Name ?? r.Supplier?.Name,
        HasSalesOrder      = false,
        LedgerDate         = r.CreatedAt,
        DocumentTypeLabel  = "Nhập kho",
        Lines = r.Lines.Select(l => new WarehouseTransactionLineDto
        {
            ProductCode   = l.Product?.Code ?? "",
            ProductName   = l.Product?.Name ?? "",
            WarehouseName = l.Warehouse?.Name ?? "",
            DebitAccount  = l.DebitAccount,
            CreditAccount = l.CreditAccount,
            Unit          = l.Product?.Unit ?? "",
            Quantity      = l.Quantity,
            UnitPrice     = l.UnitPrice,
            Amount        = l.Amount,
        }).ToList(),
    };

    private static WarehouseTransactionResponseDto MapSalesOrder(
        SalesOrder o, Dictionary<int, Product> products, Dictionary<int, Lamour.Domain.Entities.Warehouse> warehouses) => new()
    {
        Id                 = o.Id,
        TransactionType    = "Export",
        DocumentNumber     = o.DocumentNumber,
        AccountingDate     = o.AccountingDate,
        DocumentDate       = o.DocumentDate,
        Description        = string.IsNullOrWhiteSpace(o.Description) ? $"Xuất kho bán hàng {o.Customer?.Name}" : o.Description,
        TotalAmount        = o.TotalAmount,
        DeliveryOrReceiver = null, // SalesOrder không lưu tên người giao/nhận riêng (chỉ có DeliveryMethod dạng mô tả)
        ObjectName         = o.Customer?.Name,
        HasSalesOrder      = true, // dòng Xuất kho luôn phát sinh TỪ 1 Sales Order đã ghi sổ
        LedgerDate         = o.CreatedAt,
        DocumentTypeLabel  = "Xuất kho bán hàng",
        Lines = o.Lines.Where(l => !l.IsPromotion).Select(l =>
        {
            products.TryGetValue(l.ProductId, out var product);
            warehouses.TryGetValue(l.WarehouseId, out var warehouse);
            return new WarehouseTransactionLineDto
            {
                ProductCode   = l.ProductCode,
                ProductName   = l.ProductName,
                WarehouseName = warehouse?.Name ?? "",
                DebitAccount  = product?.CostAccount?.Code  ?? "632",
                CreditAccount = product?.StockAccount?.Code ?? "1561",
                Unit          = l.Unit,
                Quantity      = l.Quantity,
                UnitPrice     = l.UnitPrice,
                Amount        = l.Amount,
            };
        }).ToList(),
    };
}
