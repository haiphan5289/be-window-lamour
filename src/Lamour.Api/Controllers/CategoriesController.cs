using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Categories.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IGetCategoriesUseCase   _getAll;
    private readonly ICreateCategoryUseCase  _create;
    private readonly IUpdateCategoryUseCase  _update;
    private readonly IDeleteCategoryUseCase  _delete;

    public CategoriesController(
        IGetCategoriesUseCase  getAll,
        ICreateCategoryUseCase create,
        IUpdateCategoryUseCase update,
        IDeleteCategoryUseCase delete)
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
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto request, CancellationToken ct)
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
