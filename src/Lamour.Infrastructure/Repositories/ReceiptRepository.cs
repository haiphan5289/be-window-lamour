using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDbContext _db;

    public ReceiptRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Receipt>> GetAllAsync(CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<Receipt?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        // Reload navigations
        await _db.Entry(receipt).Reference(r => r.Customer).LoadAsync(ct);
        if (receipt.CollectorEmployeeId.HasValue)
            await _db.Entry(receipt).Reference(r => r.CollectorEmployee).LoadAsync(ct);

        return receipt;
    }

    public async Task UpdateAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Update(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Remove(receipt);
        await _db.SaveChangesAsync(ct);
    }
}
