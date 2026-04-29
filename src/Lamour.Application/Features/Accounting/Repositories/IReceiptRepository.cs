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
}
