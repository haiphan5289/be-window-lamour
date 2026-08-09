using Lamour.Application.Features.Warehouses.Dtos;
using Lamour.Application.Features.Warehouses.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IGetWarehousesUseCase   _getAll;
    private readonly ICreateWarehouseUseCase _create;
    private readonly IUpdateWarehouseUseCase _update;
    private readonly IDeleteWarehouseUseCase _delete;

    public WarehousesController(
        IGetWarehousesUseCase   getAll,
        ICreateWarehouseUseCase create,
        IUpdateWarehouseUseCase update,
        IDeleteWarehouseUseCase delete)
    {
        _getAll = getAll;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequestDto request, CancellationToken ct)
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
}
