using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetUnconfirmedByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);

    // Số chứng từ → Id của phiếu chi (dùng để gắn payment_id vào dòng sổ quỹ đã ghi sổ).
    Task<Dictionary<string, int>> GetIdsByDocumentNumbersAsync(IEnumerable<string> documentNumbers, CancellationToken ct = default);
    Task<Payment?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
    Task DeleteAsync(Payment payment, CancellationToken ct = default);
}
