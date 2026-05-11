using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/accounting/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IGetPaymentsUseCase       _getPayments;
    private readonly IGetPaymentByIdUseCase    _getPaymentById;
    private readonly ICreatePaymentUseCase     _createPayment;
    private readonly IUpdatePaymentUseCase     _updatePayment;
    private readonly IDeletePaymentUseCase     _deletePayment;
    private readonly IDuplicatePaymentUseCase  _duplicatePayment;

    public PaymentsController(
        IGetPaymentsUseCase getPayments,
        IGetPaymentByIdUseCase getPaymentById,
        ICreatePaymentUseCase createPayment,
        IUpdatePaymentUseCase updatePayment,
        IDeletePaymentUseCase deletePayment,
        IDuplicatePaymentUseCase duplicatePayment)
    {
        _getPayments       = getPayments;
        _getPaymentById    = getPaymentById;
        _createPayment     = createPayment;
        _updatePayment     = updatePayment;
        _deletePayment     = deletePayment;
        _duplicatePayment  = duplicatePayment;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments(CancellationToken ct)
        => Ok(await _getPayments.ExecuteAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPaymentById(int id, CancellationToken ct)
        => Ok(await _getPaymentById.ExecuteAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequestDto request, CancellationToken ct)
    {
        var result = await _createPayment.ExecuteAsync(request, ct);
        return Created($"/api/v1/accounting/payments/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePayment(
        int id, [FromBody] UpdatePaymentRequestDto request, CancellationToken ct)
        => Ok(await _updatePayment.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePayment(int id, CancellationToken ct)
    {
        await _deletePayment.ExecuteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/duplicate")]
    public async Task<IActionResult> DuplicatePayment(int id, CancellationToken ct)
    {
        var result = await _duplicatePayment.ExecuteAsync(id, ct);
        return Created($"/api/v1/accounting/payments/{result.Id}", result);
    }
}
