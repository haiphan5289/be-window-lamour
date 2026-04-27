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

    public WarehouseReceiptsController(
        IGetWarehouseReceiptsUseCase getAll,
        IGetWarehouseReceiptByIdUseCase getById,
        ICreateWarehouseReceiptUseCase create,
        IConfirmWarehouseReceiptUseCase confirm)
    {
        _getAll  = getAll;
        _getById = getById;
        _create  = create;
        _confirm = confirm;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
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
        return Created($"api/v1/warehouse-receipts/{result.Id}", result);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var result = await _confirm.ExecuteAsync(id, ct);
        return Ok(result);
    }
}
