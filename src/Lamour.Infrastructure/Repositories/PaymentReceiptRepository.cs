using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class PaymentReceiptRepository : IPaymentReceiptRepository
{
    private readonly AppDbContext _db;

    public PaymentReceiptRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<PaymentReceipt>> GetAllAsync(CancellationToken ct = default)
        => await _db.PaymentReceipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Employee)
            .Include(r => r.Lines)
            .ToListAsync(ct);

    public async Task<PaymentReceipt> AddAsync(PaymentReceipt receipt, CancellationToken ct = default)
    {
        _db.PaymentReceipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        // Reload navigation properties so the returned entity has Customer + Employee populated
        await _db.Entry(receipt).Reference(r => r.Customer).LoadAsync(ct);
        if (receipt.EmployeeId.HasValue)
            await _db.Entry(receipt).Reference(r => r.Employee).LoadAsync(ct);

        return receipt;
    }

    public async Task<string> GetNextReceiptNumberAsync(DateTime date, CancellationToken ct = default)
    {
        var datePart = date.ToString("yyyyMMdd");
        var prefix   = $"PT-{datePart}-";

        var count = await _db.PaymentReceipts
            .AsNoTracking()
            .CountAsync(r => r.ReceiptNumber.StartsWith(prefix), ct);

        var seq = count + 1;
        return $"{prefix}{seq:D3}";
    }
}
