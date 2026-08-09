using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ProductUnitRepository : IProductUnitRepository
{
    private readonly AppDbContext _db;

    public ProductUnitRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<ProductUnit>> GetAllAsync(CancellationToken ct = default)
        => await _db.ProductUnits.AsNoTracking().OrderBy(u => u.Name).ToListAsync(ct);

    public async Task<ProductUnit?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.ProductUnits.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => await _db.ProductUnits.AsNoTracking()
            .AnyAsync(u => u.Name.ToLower() == name.ToLower() && (excludeId == null || u.Id != excludeId), ct);

    public async Task<ProductUnit> AddAsync(ProductUnit unit, CancellationToken ct = default)
    {
        _db.ProductUnits.Add(unit);
        await _db.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<ProductUnit> UpdateAsync(ProductUnit unit, CancellationToken ct = default)
    {
        _db.ProductUnits.Update(unit);
        await _db.SaveChangesAsync(ct);
        return unit;
    }

    public async Task DeleteAsync(ProductUnit unit, CancellationToken ct = default)
    {
        _db.ProductUnits.Remove(unit);
        await _db.SaveChangesAsync(ct);
    }
}
