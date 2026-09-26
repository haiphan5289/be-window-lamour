using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface IReceiptRepository
{
    Task<IEnumerable<Receipt>> GetAllAsync(CancellationToken ct = default);
    Task<Receipt?> GetByIdAsync(int id, CancellationToken ct = default);

    // Phiếu thu CHƯA ghi sổ trong khoảng ngày (theo AccountingDate) — hiện trên sổ quỹ để Ghi sổ ngay
    // từ màn Quỹ, không làm thay đổi số tồn (giống IPaymentRepository.GetUnconfirmedByDateRangeAsync).
    Task<IEnumerable<Receipt>> GetUnconfirmedByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);

    // Số chứng từ → Id của phiếu thu (dùng để gắn receipt_id vào dòng sổ quỹ đã ghi sổ).
    Task<Dictionary<string, int>> GetIdsByDocumentNumbersAsync(IEnumerable<string> documentNumbers, CancellationToken ct = default);

    // Trong các id cho trước, id nào là phiếu thu HÀNG LOẠT (CustomerId == null) — màn Quỹ dùng để mở
    // đúng cửa sổ Phiếu thu hàng loạt thay vì cửa sổ Phiếu thu thường khi double-click/Sửa.
    Task<HashSet<int>> GetBulkReceiptIdsAsync(IEnumerable<int> receiptIds, CancellationToken ct = default);
    Task<Receipt?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default);
    Task UpdateAsync(Receipt receipt, CancellationToken ct = default);
    Task DeleteAsync(Receipt receipt, CancellationToken ct = default);

    // Số chứng từ tiếp theo dạng "{prefix}{5 digits}" — tìm số lớn nhất đang có rồi +1 (khớp
    // pattern SalesOrderRepository.GetNextCodeNumberAsync).
    Task<int> GetNextCodeNumberAsync(string prefix, CancellationToken ct = default);

    // Số tiền còn nợ hiện tại của 1 SalesOrder = GrandTotal − đã thu qua ReceiptEntry đã liên kết
    // − đã trừ qua DepositDeduction — dùng để validate server-side trước khi tạo receipt entry mới
    // (không tin remaining_amount client gửi lên, vốn chỉ là giá trị tại thời điểm search).
    Task<decimal> GetRemainingAmountAsync(int salesOrderId, CancellationToken ct = default);

    // Danh sách SalesOrder (Status=Normal) còn nợ > 0 trong khoảng ngày (theo AccountingDate),
    // filter theo NV bán hàng — dùng cho popup "Thu tiền khách hàng hàng loạt".
    Task<IEnumerable<(
        int OrderId, string DocumentNumber, DateTime AccountingDate, DateTime DocumentDate,
        int CustomerId, string CustomerCode, string CustomerName, string? Description,
        decimal GrandTotal, string? PaymentTerms, DateTime? PaymentDueDate,
        decimal RemainingAmount)>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId, CancellationToken ct = default);

    // Lấy đúng các SalesOrder theo id — KHÔNG lọc theo còn nợ/khoảng ngày (khác
    // GetOutstandingSalesOrdersAsync) — dùng để dựng lại tab "2. Chứng từ" khi Sửa 1 phiếu thu
    // hàng loạt ĐÃ LƯU: các đơn đã gắn vào phiếu này có thể hết nợ (đã thu đủ) hoặc ngoài khoảng
    // ngày tìm kiếm ban đầu, nhưng vẫn phải hiện lại để sửa số tiền dòng đó.
    Task<IEnumerable<(
        int OrderId, string DocumentNumber, DateTime AccountingDate, DateTime DocumentDate,
        int CustomerId, string CustomerCode, string CustomerName, string? Description,
        decimal GrandTotal, string? PaymentTerms, DateTime? PaymentDueDate,
        decimal RemainingAmount)>> GetSalesOrdersByIdsAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default);
}
