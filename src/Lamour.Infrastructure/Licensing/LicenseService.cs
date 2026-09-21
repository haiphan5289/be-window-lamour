using System.Globalization;
using Lamour.Application.Abstractions;
using Lamour.Application.Licensing;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.Licensing;

public class LicenseService : ILicenseService
{
    // Ngày hết hạn CỐ ĐỊNH trong build (UTC). Đổi giá trị này rồi publish lại mỗi lần cấp/gia hạn cho khách.
    // 2026-10-01 23:59:59 giờ Việt Nam (UTC+7) = 2026-10-01 16:59:59 UTC.
    public static readonly DateTime ExpiresAtUtc = new(2026, 10, 1, 16, 59, 59, DateTimeKind.Utc);

    private const string LastSeenKey = "license_last_seen_utc";
    private static readonly TimeSpan PersistInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LicenseService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool      _loaded;
    private DateTime? _lastSeenUtc;
    private DateTime? _lastPersistedUtc;

    public LicenseService(IServiceScopeFactory scopeFactory, ILogger<LicenseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task<bool> IsActiveAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var now = DateTime.UtcNow;
            if (!_loaded)
                await LoadAsync(ct);

            var status = LicenseEvaluator.Evaluate(now, _lastSeenUtc, ExpiresAtUtc);
            if (status == LicenseStatus.ClockTampered)
            {
                _logger.LogWarning("License check failed: system clock earlier than last seen time.");
                return false;
            }

            if (_lastSeenUtc is null || now > _lastSeenUtc)
                _lastSeenUtc = now;

            if (_lastPersistedUtc is null || _lastSeenUtc - _lastPersistedUtc >= PersistInterval)
                await PersistAsync(ct);

            if (status == LicenseStatus.Expired)
                _logger.LogWarning("License expired at {ExpiresAt:u}.", ExpiresAtUtc);

            return status == LicenseStatus.Active;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var state = await db.AppStates.AsNoTracking().FirstOrDefaultAsync(x => x.Key == LastSeenKey, ct);
            if (state is not null
                && DateTime.TryParse(state.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
            {
                _lastSeenUtc      = parsed.ToUniversalTime();
                _lastPersistedUtc = _lastSeenUtc;
            }
            _loaded = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Không load được DB thì vẫn kiểm tra theo đồng hồ hệ thống, thử load lại ở request sau.
            _logger.LogError(ex, "Could not load license state.");
        }
    }

    private async Task PersistAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var value = _lastSeenUtc!.Value.ToString("O", CultureInfo.InvariantCulture);
            var state = await db.AppStates.FirstOrDefaultAsync(x => x.Key == LastSeenKey, ct);
            if (state is null)
                db.AppStates.Add(new AppState { Key = LastSeenKey, Value = value, UpdatedAt = DateTime.UtcNow });
            else
            {
                state.Value     = value;
                state.UpdatedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(ct);
            _lastPersistedUtc = _lastSeenUtc;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Could not persist license state.");
        }
    }
}
