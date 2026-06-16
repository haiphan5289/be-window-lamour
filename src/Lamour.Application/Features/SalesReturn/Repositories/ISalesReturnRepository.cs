namespace Lamour.Application.Features.SalesReturn.Repositories;

using SalesReturnEntity = Lamour.Domain.Entities.SalesReturn;

public interface ISalesReturnRepository
{
    Task<IEnumerable<SalesReturnEntity>> GetAllAsync(CancellationToken ct = default);
    Task<SalesReturnEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesReturnEntity?> GetByIdTrackedAsync(int id, CancellationToken ct = default);
    Task<SalesReturnEntity> AddAsync(SalesReturnEntity salesReturn, CancellationToken ct = default);
    Task UpdateAsync(SalesReturnEntity salesReturn, CancellationToken ct = default);
    Task DeleteAsync(SalesReturnEntity salesReturn, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> GetNextCodeNumberAsync(CancellationToken ct = default);
}
