namespace Lamour.Application.Abstractions;

public interface ILicenseService
{
    // true = ứng dụng còn được phép hoạt động; false = đã hết hạn hoặc phát hiện lùi đồng hồ.
    Task<bool> IsActiveAsync(CancellationToken ct = default);
}
