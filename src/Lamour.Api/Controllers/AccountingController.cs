using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/accounting")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IGetCashLedgerUseCase         _getCashLedger;
    private readonly IGetPaymentReceiptsUseCase    _getPaymentReceipts;
    private readonly ICreatePaymentReceiptUseCase  _createPaymentReceipt;

    public AccountingController(
        IGetCashLedgerUseCase getCashLedger,
        IGetPaymentReceiptsUseCase getPaymentReceipts,
        ICreatePaymentReceiptUseCase createPaymentReceipt)
    {
        _getCashLedger        = getCashLedger;
        _getPaymentReceipts   = getPaymentReceipts;
        _createPaymentReceipt = createPaymentReceipt;
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

    [HttpGet("payment-receipts")]
    public async Task<IActionResult> GetPaymentReceipts(CancellationToken ct)
    {
        var result = await _getPaymentReceipts.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost("payment-receipts")]
    public async Task<IActionResult> CreatePaymentReceipt(
        [FromBody] CreatePaymentReceiptRequestDto request, CancellationToken ct)
    {
        var result = await _createPaymentReceipt.ExecuteAsync(request, ct);
        return Created($"api/v1/accounting/payment-receipts/{result.Id}", result);
    }
}
