using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _db.Payments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Payments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Payments
            .Include(p => p.Supplier)
            .Include(p => p.PaymentEmployee)
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        // Reload navigations
        await _db.Entry(payment).Reference(p => p.Supplier).LoadAsync(ct);
        if (payment.PaymentEmployeeId.HasValue)
            await _db.Entry(payment).Reference(p => p.PaymentEmployee).LoadAsync(ct);

        return payment;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync(ct);
    }
}
