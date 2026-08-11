using Lamour.Application.Features.Departments.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;

    public DepartmentRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default)
        => await _db.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);

    public async Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => await _db.Departments.AsNoTracking()
            .AnyAsync(d => d.Name.ToLower() == name.ToLower() && (excludeId == null || d.Id != excludeId), ct);

    public async Task<bool> IsInUseAsync(int departmentId, CancellationToken ct = default)
        => await _db.ExpenseCategories.AsNoTracking().AnyAsync(e => e.DepartmentId == departmentId, ct);

    public async Task<Department> AddAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Add(department);
        await _db.SaveChangesAsync(ct);
        return department;
    }

    public async Task<Department> UpdateAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Update(department);
        await _db.SaveChangesAsync(ct);
        return department;
    }

    public async Task DeleteAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Remove(department);
        await _db.SaveChangesAsync(ct);
    }
}
