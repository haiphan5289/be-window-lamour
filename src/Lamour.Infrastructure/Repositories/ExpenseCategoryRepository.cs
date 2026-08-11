using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _db;

    public ExpenseCategoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync(CancellationToken ct = default)
        => await _db.ExpenseCategories.AsNoTracking()
            .Include(e => e.Department)
            .OrderBy(e => e.Code)
            .ToListAsync(ct);

    public async Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.ExpenseCategories.AsNoTracking()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.ExpenseCategories.AsNoTracking()
            .AnyAsync(e => e.Code.ToLower() == code.ToLower() && (excludeId == null || e.Id != excludeId), ct);

    public async Task<ExpenseCategory> AddAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _db.ExpenseCategories.Add(category);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(category.Id, ct) ?? category;
    }

    public async Task<ExpenseCategory> UpdateAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _db.ExpenseCategories.Update(category);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(category.Id, ct) ?? category;
    }

    public async Task DeleteAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _db.ExpenseCategories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
