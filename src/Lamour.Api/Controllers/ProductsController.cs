using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
// TODO: restore [Authorize] once WPF auth flow is wired up
public class ProductsController : ControllerBase
{
    private readonly IGetProductsUseCase      _getAll;
    private readonly ICreateProductUseCase    _create;
    private readonly IUpdateProductUseCase    _update;
    private readonly IDeleteProductUseCase    _delete;
    private readonly IDuplicateProductUseCase _duplicate;

    public ProductsController(
        IGetProductsUseCase      getAll,
        ICreateProductUseCase    create,
        IUpdateProductUseCase    update,
        IDeleteProductUseCase    delete,
        IDuplicateProductUseCase duplicate)
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
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequestDto request, CancellationToken ct)
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
