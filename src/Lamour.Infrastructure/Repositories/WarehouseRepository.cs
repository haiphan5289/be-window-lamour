using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _db;

    public WarehouseRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken ct = default)
        => await _db.Warehouses.AsNoTracking().OrderBy(w => w.Code).ToListAsync(ct);

    public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.Warehouses.AsNoTracking()
            .AnyAsync(w => w.Code.ToLower() == code.ToLower() && (excludeId == null || w.Id != excludeId), ct);

    public async Task<bool> IsInUseAsync(int warehouseId, CancellationToken ct = default)
        => await _db.WarehouseReceiptLines.AsNoTracking().AnyAsync(l => l.WarehouseId == warehouseId, ct)
        || await _db.Products.AsNoTracking().AnyAsync(p => p.DefaultWarehouseId == warehouseId, ct);

    public async Task<Warehouse> AddAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(ct);
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        _db.Warehouses.Update(warehouse);
        await _db.SaveChangesAsync(ct);
        return warehouse;
    }

    public async Task DeleteAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        _db.Warehouses.Remove(warehouse);
        await _db.SaveChangesAsync(ct);
    }
}
