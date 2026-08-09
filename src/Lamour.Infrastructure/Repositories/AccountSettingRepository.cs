using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class AccountSettingRepository : IAccountSettingRepository
{
    private readonly AppDbContext _db;

    public AccountSettingRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<AccountSetting>> GetAllAsync(CancellationToken ct = default)
        => await _db.AccountSettings.AsNoTracking().OrderBy(a => a.Code).ToListAsync(ct);

    public async Task<AccountSetting?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.AccountSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.AccountSettings.AsNoTracking()
            .AnyAsync(a => a.Code.ToLower() == code.ToLower() && (excludeId == null || a.Id != excludeId), ct);

    public async Task<AccountSetting> AddAsync(AccountSetting account, CancellationToken ct = default)
    {
        _db.AccountSettings.Add(account);
        await _db.SaveChangesAsync(ct);
        return account;
    }

    public async Task<AccountSetting> UpdateAsync(AccountSetting account, CancellationToken ct = default)
    {
        _db.AccountSettings.Update(account);
        await _db.SaveChangesAsync(ct);
        return account;
    }

    public async Task DeleteAsync(AccountSetting account, CancellationToken ct = default)
    {
        _db.AccountSettings.Remove(account);
        await _db.SaveChangesAsync(ct);
    }
}
