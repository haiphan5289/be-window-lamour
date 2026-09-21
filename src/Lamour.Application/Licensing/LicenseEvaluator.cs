namespace Lamour.Application.Licensing;

public enum LicenseStatus
{
    Active,
    Expired,
    ClockTampered,
}

public static class LicenseEvaluator
{
    // Cho phép lệch nhẹ (đồng bộ giờ mạng, đổi múi giờ tự động) trước khi coi là lùi đồng hồ.
    public static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    public static LicenseStatus Evaluate(DateTime nowUtc, DateTime? lastSeenUtc, DateTime expiresAtUtc)
    {
        if (lastSeenUtc.HasValue && nowUtc < lastSeenUtc.Value - ClockTolerance)
            return LicenseStatus.ClockTampered;

        var effectiveNow = lastSeenUtc.HasValue && lastSeenUtc.Value > nowUtc ? lastSeenUtc.Value : nowUtc;
        return effectiveNow >= expiresAtUtc ? LicenseStatus.Expired : LicenseStatus.Active;
    }
}
