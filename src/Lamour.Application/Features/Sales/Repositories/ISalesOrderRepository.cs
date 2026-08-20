using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Sales.Repositories;

public interface ISalesOrderRepository
{
    Task<IEnumerable<SalesOrder>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default);
    Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesOrder?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task DeleteAsync(SalesOrder order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetNextCodeNumberAsync(string prefix, CancellationToken ct = default);

    Task<IEnumerable<SalesOrderLine>> GetReportLinesAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
