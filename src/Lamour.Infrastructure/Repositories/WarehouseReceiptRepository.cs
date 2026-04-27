using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class WarehouseReceiptRepository : IWarehouseReceiptRepository
{
    private readonly AppDbContext _db;

    public WarehouseReceiptRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<WarehouseReceipt>> GetAllAsync(CancellationToken ct = default)
        => await _db.WarehouseReceipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Product)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Warehouse)
            .OrderByDescending(r => r.DocumentDate)
            .ToListAsync(ct);

    public async Task<WarehouseReceipt?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.WarehouseReceipts
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Product)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Warehouse)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<WarehouseReceipt> AddAsync(WarehouseReceipt receipt, CancellationToken ct = default)
    {
        _db.WarehouseReceipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(receipt).Reference(r => r.Customer).LoadAsync(ct);
        if (receipt.EmployeeId.HasValue)
            await _db.Entry(receipt).Reference(r => r.Employee).LoadAsync(ct);

        foreach (var line in receipt.Lines)
        {
            await _db.Entry(line).Reference(l => l.Product).LoadAsync(ct);
            await _db.Entry(line).Reference(l => l.Warehouse).LoadAsync(ct);
        }

        return receipt;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<string> GetNextReceiptNumberAsync(DateTime date, CancellationToken ct = default)
    {
        var datePart = date.ToString("yyyyMMdd");
        var prefix   = $"NK-{datePart}-";

        var count = await _db.WarehouseReceipts
            .AsNoTracking()
            .CountAsync(r => r.ReceiptNumber.StartsWith(prefix), ct);

        return $"{prefix}{count + 1:D3}";
    }
}
