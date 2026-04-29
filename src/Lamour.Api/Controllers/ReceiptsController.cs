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

    public ReceiptsController(
        IGetReceiptsUseCase getReceipts,
        IGetReceiptByIdUseCase getReceiptById,
        ICreateReceiptUseCase createReceipt,
        IUpdateReceiptUseCase updateReceipt,
        IDeleteReceiptUseCase deleteReceipt)
    {
        _getReceipts    = getReceipts;
        _getReceiptById = getReceiptById;
        _createReceipt  = createReceipt;
        _updateReceipt  = updateReceipt;
        _deleteReceipt  = deleteReceipt;
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
        return Created($"api/v1/accounting/receipts/{result.Id}", result);
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
}
