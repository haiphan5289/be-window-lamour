using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.ProductUnits.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/product-units")]
[Authorize]
public class ProductUnitsController : ControllerBase
{
    private readonly IGetProductUnitsUseCase   _getAll;
    private readonly ICreateProductUnitUseCase _create;
    private readonly IUpdateProductUnitUseCase _update;
    private readonly IDeleteProductUnitUseCase _delete;

    public ProductUnitsController(
        IGetProductUnitsUseCase   getAll,
        ICreateProductUnitUseCase create,
        IUpdateProductUnitUseCase update,
        IDeleteProductUnitUseCase delete)
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
    public async Task<IActionResult> Create([FromBody] CreateProductUnitRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductUnitRequestDto request, CancellationToken ct)
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
