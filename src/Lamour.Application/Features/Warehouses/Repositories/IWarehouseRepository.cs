using WarehouseEntity = Lamour.Domain.Entities.Warehouse;

namespace Lamour.Application.Features.Warehouses.Repositories;

public interface IWarehouseRepository
{
    Task<IEnumerable<WarehouseEntity>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int warehouseId, CancellationToken ct = default);
    Task<WarehouseEntity> AddAsync(WarehouseEntity warehouse, CancellationToken ct = default);
    Task<WarehouseEntity> UpdateAsync(WarehouseEntity warehouse, CancellationToken ct = default);
    Task DeleteAsync(WarehouseEntity warehouse, CancellationToken ct = default);
}
