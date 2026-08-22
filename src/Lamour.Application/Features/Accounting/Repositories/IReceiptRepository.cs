using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface IReceiptRepository
{
    Task<IEnumerable<Receipt>> GetAllAsync(CancellationToken ct = default);
    Task<Receipt?> GetByIdAsync(int id, CancellationToken ct = default);
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
        decimal RemainingAmount)>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId, CancellationToken ct = default);
}
