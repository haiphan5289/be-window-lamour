using Lamour.Application.Licensing;
using Xunit;

namespace Lamour.Application.Tests.Licensing;

public class LicenseEvaluatorTests
{
    private static readonly DateTime Expires = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Before_expiry_is_active()
        => Assert.Equal(LicenseStatus.Active,
            LicenseEvaluator.Evaluate(Expires.AddDays(-3), null, Expires));

    [Fact]
    public void At_or_after_expiry_is_expired()
    {
        Assert.Equal(LicenseStatus.Expired, LicenseEvaluator.Evaluate(Expires, null, Expires));
        Assert.Equal(LicenseStatus.Expired, LicenseEvaluator.Evaluate(Expires.AddDays(5), null, Expires));
    }

    [Fact]
    public void Clock_rolled_back_beyond_tolerance_is_tampered()
    {
        var lastSeen = Expires.AddDays(-1);
        Assert.Equal(LicenseStatus.ClockTampered,
            LicenseEvaluator.Evaluate(lastSeen.AddHours(-2), lastSeen, Expires));
    }

    [Fact]
    public void Small_clock_drift_within_tolerance_is_allowed()
    {
        var lastSeen = Expires.AddDays(-1);
        Assert.Equal(LicenseStatus.Active,
            LicenseEvaluator.Evaluate(lastSeen.AddMinutes(-2), lastSeen, Expires));
    }

    [Fact]
    public void Last_seen_after_expiry_stays_expired_even_if_now_is_slightly_earlier()
    {
        var lastSeen = Expires.AddMinutes(1);
        Assert.Equal(LicenseStatus.Expired,
            LicenseEvaluator.Evaluate(lastSeen.AddMinutes(-1), lastSeen, Expires));
    }
}
