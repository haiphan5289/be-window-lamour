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

    public AccountingController(IGetCashLedgerUseCase getCashLedger)
    {
        _getCashLedger = getCashLedger;
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
