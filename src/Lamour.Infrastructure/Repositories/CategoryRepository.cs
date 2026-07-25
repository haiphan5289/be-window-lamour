using Lamour.Application.Features.Categories.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => await _db.Categories.AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId), ct);

    public async Task<bool> IsInUseAsync(int categoryId, CancellationToken ct = default)
        => await _db.Products.AsNoTracking().AnyAsync(p => p.CategoryId == categoryId, ct);

    public async Task<Category> AddAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
