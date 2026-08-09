using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class DepositRepository : IDepositRepository
{
    private readonly AppDbContext _db;

    public DepositRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Deposit>> GetAllAsync(CancellationToken ct = default)
        => await _db.Deposits
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.Employee)
            .Include(d => d.Deductions).ThenInclude(x => x.SalesOrder).ThenInclude(o => o.Employee)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<Deposit?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Deposits
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.Employee)
            .Include(d => d.Deductions).ThenInclude(x => x.SalesOrder).ThenInclude(o => o.Employee)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Deposit?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Deposits
            .Include(d => d.Customer)
            .Include(d => d.Employee)
            .Include(d => d.Deductions).ThenInclude(x => x.SalesOrder).ThenInclude(o => o.Employee)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<Deposit>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default)
        => await _db.Deposits
            .AsNoTracking()
            .Include(d => d.Customer)
            .Where(d => d.CustomerId == customerId && d.RemainingBalance > 0)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<Deposit> AddAsync(Deposit deposit, CancellationToken ct = default)
    {
        _db.Deposits.Add(deposit);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(deposit).Reference(d => d.Customer).LoadAsync(ct);
        if (deposit.EmployeeId.HasValue)
            await _db.Entry(deposit).Reference(d => d.Employee).LoadAsync(ct);

        return deposit;
    }

    public async Task UpdateAsync(Deposit deposit, CancellationToken ct = default)
    {
        _db.Deposits.Update(deposit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Deposit deposit, CancellationToken ct = default)
    {
        _db.Deposits.Remove(deposit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<int> GetNextCodeNumberAsync(CancellationToken ct = default)
    {
        const string prefix = "DC";
        var numbers = await _db.Deposits
            .AsNoTracking()
            .Select(d => d.DocumentNumber)
            .Where(n => n.StartsWith(prefix))
            .ToListAsync(ct);

        var max = numbers
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return max + 1;
    }
}
