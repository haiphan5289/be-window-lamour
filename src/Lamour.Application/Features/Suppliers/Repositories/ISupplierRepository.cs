using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Suppliers.Repositories;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<Supplier> AddAsync(Supplier supplier, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Supplier> suppliers, CancellationToken ct = default);
    Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken ct = default);
    Task DeleteAsync(Supplier supplier, CancellationToken ct = default);
}
