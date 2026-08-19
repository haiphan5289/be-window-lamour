using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _db;

    public SupplierRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default)
        => await _db.Suppliers.AsNoTracking().ToListAsync(ct);

    public async Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.Suppliers.AsNoTracking()
            .AnyAsync(s => s.Code.ToLower() == code.ToLower() && (excludeId == null || s.Id != excludeId), ct);

    public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task AddRangeAsync(IEnumerable<Supplier> suppliers, CancellationToken ct = default)
    {
        _db.Suppliers.AddRange(suppliers);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task DeleteAsync(Supplier supplier, CancellationToken ct = default)
    {
        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync(ct);
    }
}
