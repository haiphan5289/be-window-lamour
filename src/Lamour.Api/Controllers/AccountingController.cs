using Lamour.Application.Features.Accounting.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/accounting")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IGetCashLedgerUseCase _getCashLedger;
    private readonly IGetCashLedgerDetailReportUseCase _getCashLedgerDetailReport;

    public AccountingController(
        IGetCashLedgerUseCase getCashLedger,
        IGetCashLedgerDetailReportUseCase getCashLedgerDetailReport)
    {
        _getCashLedger = getCashLedger;
        _getCashLedgerDetailReport = getCashLedgerDetailReport;
    }

    // Báo cáo "Sổ kế toán chi tiết quỹ tiền mặt". account_codes: các mã TK cách nhau dấu phẩy
    // (vd "111,1111"); bỏ trống = mọi TK tiền mặt (111*).
    [HttpGet("reports/cash-ledger-detail")]
    public async Task<IActionResult> GetCashLedgerDetailReport(
        [FromQuery] DateTime from_date,
        [FromQuery] DateTime to_date,
        [FromQuery] string? account_codes,
        [FromQuery] bool merge_similar,
        [FromQuery] bool order_by_created,
        CancellationToken ct)
    {
        var codes = (account_codes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = await _getCashLedgerDetailReport.ExecuteAsync(
            from_date, to_date, codes, merge_similar, order_by_created, ct);
        return Ok(result);
    }

    [HttpGet("cash-ledger")]
    public async Task<IActionResult> GetCashLedger(
        [FromQuery] DateTime from_date,
        [FromQuery] DateTime to_date,
        CancellationToken ct)
    {
        var result = await _getCashLedger.ExecuteAsync(from_date, to_date, ct);
        return Ok(result);
    }
}
