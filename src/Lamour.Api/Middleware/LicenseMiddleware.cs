using Lamour.Application.Abstractions;

namespace Lamour.Api.Middleware;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ILicenseService license)
    {
        if (await license.IsActiveAsync(context.RequestAborted))
        {
            await _next(context);
            return;
        }

        // Thông báo chung chung, cố ý không nói rõ lý do (hết hạn/lùi đồng hồ).
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(
            new { error = "Hệ thống tạm thời không khả dụng. Vui lòng liên hệ quản trị viên." },
            context.RequestAborted);
    }
}
