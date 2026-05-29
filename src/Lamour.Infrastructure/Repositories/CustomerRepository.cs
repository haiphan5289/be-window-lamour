using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default)
        => await _db.Customers.AsNoTracking().ToListAsync(ct);

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        var codes = await _db.Customers.AsNoTracking().Select(c => c.Code).ToListAsync(ct);
        var maxNum = codes
            .Where(c => c.StartsWith("KH") && c.Length == 7 && int.TryParse(c.Substring(2), out _))
            .Select(c => int.Parse(c.Substring(2)))
            .DefaultIfEmpty(0)
            .Max();
        return $"KH{(maxNum + 1):D5}";
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task AddRangeAsync(IEnumerable<Customer> customers, CancellationToken ct = default)
    {
        _db.Customers.AddRange(customers);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task DeleteAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(ct);
    }
}
