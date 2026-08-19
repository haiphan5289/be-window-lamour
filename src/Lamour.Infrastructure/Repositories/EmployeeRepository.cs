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

    public async Task<Employee?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        => await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Phone == phone, ct);

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        var codes = await _db.Employees.AsNoTracking().Select(e => e.Code).ToListAsync(ct);
        var maxNum = codes
            .Where(c => c.StartsWith("NV") && c.Length == 7 && int.TryParse(c.Substring(2), out _))
            .Select(c => int.Parse(c.Substring(2)))
            .DefaultIfEmpty(0)
            .Max();
        return $"NV{(maxNum + 1):D5}";
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct = default)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);
        return employee;
    }

    public async Task AddRangeAsync(IEnumerable<Employee> employees, CancellationToken ct = default)
    {
        _db.Employees.AddRange(employees);
        await _db.SaveChangesAsync(ct);
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
