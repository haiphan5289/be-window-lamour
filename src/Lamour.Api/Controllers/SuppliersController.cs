using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IGetSuppliersUseCase      _getAll;
    private readonly ICreateSupplierUseCase    _create;
    private readonly IUpdateSupplierUseCase    _update;
    private readonly IDeleteSupplierUseCase    _delete;
    private readonly IDuplicateSupplierUseCase _duplicate;

    public SuppliersController(
        IGetSuppliersUseCase      getAll,
        ICreateSupplierUseCase    create,
        IUpdateSupplierUseCase    update,
        IDeleteSupplierUseCase    delete,
        IDuplicateSupplierUseCase duplicate)
    {
        _getAll    = getAll;
        _create    = create;
        _update    = update;
        _delete    = delete;
        _duplicate = duplicate;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequestDto request, CancellationToken ct)
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

    [HttpPost("{id:int}/duplicate")]
    public async Task<IActionResult> Duplicate(int id, CancellationToken ct)
    {
        var result = await _duplicate.ExecuteAsync(id, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }
}
