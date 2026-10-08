using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse-receipts")]
[Authorize]
public class WarehouseReceiptsController : ControllerBase
{
    private readonly IGetWarehouseReceiptsUseCase    _getAll;
    private readonly IGetWarehouseReceiptByIdUseCase _getById;
    private readonly ICreateWarehouseReceiptUseCase  _create;
    private readonly IConfirmWarehouseReceiptUseCase _confirm;
    private readonly IUpdateWarehouseReceiptUseCase  _update;
    private readonly IUnconfirmWarehouseReceiptUseCase _unconfirm;
    private readonly IDeleteWarehouseReceiptUseCase  _delete;
    private readonly IGetNextWarehouseReceiptNumberUseCase _getNextNumber;

    public WarehouseReceiptsController(
        IGetWarehouseReceiptsUseCase getAll,
        IGetWarehouseReceiptByIdUseCase getById,
        ICreateWarehouseReceiptUseCase create,
        IConfirmWarehouseReceiptUseCase confirm,
        IUpdateWarehouseReceiptUseCase update,
        IUnconfirmWarehouseReceiptUseCase unconfirm,
        IDeleteWarehouseReceiptUseCase delete,
        IGetNextWarehouseReceiptNumberUseCase getNextNumber)
    {
        _getAll    = getAll;
        _getById   = getById;
        _create    = create;
        _confirm   = confirm;
        _update    = update;
        _unconfirm = unconfirm;
        _delete    = delete;
        _getNextNumber = getNextNumber;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    // Số phiếu xem trước cho form "Phiếu nhập kho" — số thật vẫn sinh lúc Create.
    [HttpGet("next-number")]
    public async Task<IActionResult> NextNumber(CancellationToken ct)
    {
        var code = await _getNextNumber.ExecuteAsync(ct);
        return Ok(new { code });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getById.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseReceiptRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return Created($"/api/v1/warehouse-receipts/{result.Id}", result);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var result = await _confirm.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateWarehouseReceiptRequestDto request, CancellationToken ct)
    {
        var result = await _update.ExecuteAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/unconfirm")]
    public async Task<IActionResult> Unconfirm(int id, CancellationToken ct)
    {
        var result = await _unconfirm.ExecuteAsync(id, ct);
        return Ok(result);
    }
}
