namespace Lamour.Application.Features.Sales;

// 2026-11-16: thiếu tồn kho không còn chặn lưu chứng từ (giống MISA) — đơn được lưu ở trạng thái
// Treo (Held), KHÔNG trừ kho, và message này được trả về qua SalesOrderResponseDto.StockWarning.
// Dùng chung cho Create/Update/ConfirmSalesOrderUseCase để giữ đúng 1 format dòng.
internal static class SalesOrderStockWarning
{
    public static string Build(IReadOnlyCollection<string> stockErrors)
        => "Các sản phẩm không đủ tồn kho:\n" + string.Join("\n", stockErrors);
}
