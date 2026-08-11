using Lamour.Application.Features.ExpenseCategories.Dtos;
using Lamour.Application.Features.ExpenseCategories.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/expense-categories")]
[Authorize]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IGetExpenseCategoriesUseCase   _getAll;
    private readonly ICreateExpenseCategoryUseCase _create;
    private readonly IUpdateExpenseCategoryUseCase _update;
    private readonly IDeleteExpenseCategoryUseCase _delete;

    public ExpenseCategoriesController(
        IGetExpenseCategoriesUseCase   getAll,
        ICreateExpenseCategoryUseCase create,
        IUpdateExpenseCategoryUseCase update,
        IDeleteExpenseCategoryUseCase delete)
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
    public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseCategoryRequestDto request, CancellationToken ct)
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
