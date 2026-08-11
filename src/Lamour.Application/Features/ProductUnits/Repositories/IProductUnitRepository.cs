using Lamour.Domain.Entities;

namespace Lamour.Application.Features.ProductUnits.Repositories;

public interface IProductUnitRepository
{
    Task<IEnumerable<ProductUnit>> GetAllAsync(CancellationToken ct = default);
    Task<ProductUnit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int productUnitId, CancellationToken ct = default);
    Task<ProductUnit> AddAsync(ProductUnit unit, CancellationToken ct = default);
    Task<ProductUnit> UpdateAsync(ProductUnit unit, CancellationToken ct = default);
    Task DeleteAsync(ProductUnit unit, CancellationToken ct = default);
}
