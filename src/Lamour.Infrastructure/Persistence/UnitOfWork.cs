using Lamour.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lamour.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(AppDbContext db) => _db = db;

    public async Task BeginAsync(CancellationToken ct = default)
        => _tx = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_tx is not null)
            await _tx.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx is not null)
            await _tx.RollbackAsync(ct);
    }
}
