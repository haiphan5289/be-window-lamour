using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Warehouse.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken ct = default);
}
