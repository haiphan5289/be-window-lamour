using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Deposits.Repositories;

public interface IDepositRepository
{
    Task<IEnumerable<Deposit>> GetAllAsync(CancellationToken ct = default);
    Task<Deposit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Deposit?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Deposit>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
    Task<Deposit?> GetBySourceSalesOrderIdAsync(int salesOrderId, CancellationToken ct = default);
    Task<Deposit> AddAsync(Deposit deposit, CancellationToken ct = default);
    Task UpdateAsync(Deposit deposit, CancellationToken ct = default);
    Task DeleteAsync(Deposit deposit, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetNextCodeNumberAsync(CancellationToken ct = default);
}
