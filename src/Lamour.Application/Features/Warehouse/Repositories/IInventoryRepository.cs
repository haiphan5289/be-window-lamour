using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Warehouse.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);

    // Returns confirmed import totals per product for the given date range, filtered theo warehouseIds
    // (null/empty = tất cả kho). Key = ProductId, Value = (total quantity, total amount, latest accounting date).
    Task<Dictionary<int, (int Qty, decimal Value, DateTime? LatestDate)>> GetImportsByProductAsync(
        DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default);

    // Net export qty (Sales − SalesReturn) theo product trong khoảng ngày, filter theo warehouseIds.
    Task<Dictionary<int, int>> GetExportQtyByProductAsync(
        DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default);

    // Tổng tồn kho hiện tại theo product (SUM ProductWarehouseStock.Quantity), filter theo warehouseIds.
    Task<Dictionary<int, int>> GetClosingQtyByProductAsync(
        IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default);

    // Từng dòng giao dịch (Nhập kho / Xuất kho bán hàng / Hàng bán bị trả lại) của MỘT sản phẩm
    // trong khoảng ngày, filter theo warehouseIds — dùng cho "Sổ chi tiết vật tư hàng hóa" (drill-down
    // từ Tổng hợp tồn kho). Value/ImportValue chỉ tính sẵn cho dòng Nhập (từ Amount thật trên phiếu
    // nhập, khớp cách GetImportsByProductAsync tính) — dòng Xuất/Trả lại chỉ trả Qty, UseCase tự nhân
    // với Product.CostPrice hiện tại (khớp cách GetExportQtyByProductAsync/UseCase tính ExportValue).
    Task<IEnumerable<(
        DateTime AccountingDate, DateTime DocumentDate, string DocumentNumber, string DocumentType,
        int? SourceId, string? Description, string Unit,
        int ImportQty, decimal ImportValue, int ExportQty)>> GetTransactionLinesByProductAsync(
        int productId, DateOnly fromDate, DateOnly toDate, IReadOnlyList<int>? warehouseIds = null, CancellationToken ct = default);
}
