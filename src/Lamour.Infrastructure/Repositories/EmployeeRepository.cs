using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        => await _db.Employees.AsNoTracking().ToListAsync(ct);

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct = default)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync(ct);
        return employee;
    }

    public async Task DeleteAsync(Employee employee, CancellationToken ct = default)
    {
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync(ct);
    }
}
