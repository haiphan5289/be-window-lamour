using Lamour.Domain.Entities;

namespace Lamour.Application.Features.AccountSettings.Repositories;

public interface IAccountSettingRepository
{
    Task<IEnumerable<AccountSetting>> GetAllAsync(CancellationToken ct = default);
    Task<AccountSetting?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int accountSettingId, CancellationToken ct = default);
    Task<AccountSetting> AddAsync(AccountSetting account, CancellationToken ct = default);
    Task<AccountSetting> UpdateAsync(AccountSetting account, CancellationToken ct = default);
    Task DeleteAsync(AccountSetting account, CancellationToken ct = default);
}
