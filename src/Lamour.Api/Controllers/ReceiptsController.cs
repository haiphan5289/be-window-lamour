using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/accounting/receipts")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IGetReceiptsUseCase      _getReceipts;
    private readonly IGetReceiptByIdUseCase   _getReceiptById;
    private readonly ICreateReceiptUseCase    _createReceipt;
    private readonly IUpdateReceiptUseCase    _updateReceipt;
    private readonly IDeleteReceiptUseCase    _deleteReceipt;
    private readonly IConfirmReceiptUseCase   _confirmReceipt;
    private readonly IUnconfirmReceiptUseCase _unconfirmReceipt;
    private readonly IGetNextReceiptCodeUseCase        _getNextCode;
    private readonly IGetOutstandingSalesOrdersUseCase _getOutstandingOrders;
    private readonly ICreateBulkCustomerReceiptUseCase _createBulkReceipt;

    public ReceiptsController(
        IGetReceiptsUseCase getReceipts,
        IGetReceiptByIdUseCase getReceiptById,
        ICreateReceiptUseCase createReceipt,
        IUpdateReceiptUseCase updateReceipt,
        IDeleteReceiptUseCase deleteReceipt,
        IConfirmReceiptUseCase confirmReceipt,
        IUnconfirmReceiptUseCase unconfirmReceipt,
        IGetNextReceiptCodeUseCase getNextCode,
        IGetOutstandingSalesOrdersUseCase getOutstandingOrders,
        ICreateBulkCustomerReceiptUseCase createBulkReceipt)
    {
        _getReceipts    = getReceipts;
        _getReceiptById = getReceiptById;
        _createReceipt  = createReceipt;
        _updateReceipt  = updateReceipt;
        _deleteReceipt  = deleteReceipt;
        _confirmReceipt        = confirmReceipt;
        _unconfirmReceipt      = unconfirmReceipt;
        _getNextCode           = getNextCode;
        _getOutstandingOrders  = getOutstandingOrders;
        _createBulkReceipt     = createBulkReceipt;
    }

    [HttpGet]
    public async Task<IActionResult> GetReceipts(CancellationToken ct)
        => Ok(await _getReceipts.ExecuteAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReceiptById(int id, CancellationToken ct)
        => Ok(await _getReceiptById.ExecuteAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> CreateReceipt(
        [FromBody] CreateReceiptRequestDto request, CancellationToken ct)
    {
        var result = await _createReceipt.ExecuteAsync(request, ct);
        return Created($"/api/v1/accounting/receipts/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReceipt(
        int id, [FromBody] UpdateReceiptRequestDto request, CancellationToken ct)
        => Ok(await _updateReceipt.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReceipt(int id, CancellationToken ct)
    {
        await _deleteReceipt.ExecuteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmReceipt(int id, CancellationToken ct)
        => Ok(await _confirmReceipt.ExecuteAsync(id, ct));

    [HttpPost("{id:int}/unconfirm")]
    public async Task<IActionResult> UnconfirmReceipt(int id, CancellationToken ct)
        => Ok(await _unconfirmReceipt.ExecuteAsync(id, ct));

    [HttpGet("next-code")]
    public async Task<IActionResult> GetNextCode(CancellationToken ct)
        => Ok(new { code = await _getNextCode.ExecuteAsync(ct) });

    // Popup "Thu tiền khách hàng hàng loạt" — danh sách chứng từ bán hàng còn nợ khớp filter.
    [HttpGet("outstanding-orders")]
    public async Task<IActionResult> GetOutstandingOrders(
        [FromQuery] DateOnly from_date,
        [FromQuery] DateOnly to_date,
        [FromQuery] int? employee_id,
        CancellationToken ct)
        => Ok(await _getOutstandingOrders.ExecuteAsync(from_date, to_date, employee_id, ct));

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkCustomerReceipt(
        [FromBody] CreateBulkCustomerReceiptRequestDto request, CancellationToken ct)
        => Ok(await _createBulkReceipt.ExecuteAsync(request, ct));
}
