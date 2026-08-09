using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Deposits.Repositories;

public interface IDepositDeductionRepository
{
    Task<IEnumerable<DepositDeduction>> GetAllAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);

    Task<DepositDeduction?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DepositDeduction?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<DepositDeduction> AddAsync(DepositDeduction deduction, CancellationToken ct = default);
    Task DeleteAsync(DepositDeduction deduction, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetNextCodeNumberAsync(CancellationToken ct = default);
}
