using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly IGetSalesOrdersUseCase    _getAll;
    private readonly IGetSalesOrderByIdUseCase _getById;
    private readonly ICreateSalesOrderUseCase  _create;
    private readonly IUpdateSalesOrderUseCase  _update;
    private readonly IDeleteSalesOrderUseCase  _delete;

    public SalesOrdersController(
        IGetSalesOrdersUseCase    getAll,
        IGetSalesOrderByIdUseCase getById,
        ICreateSalesOrderUseCase  create,
        IUpdateSalesOrderUseCase  update,
        IDeleteSalesOrderUseCase  delete)
    {
        _getAll  = getAll;
        _getById = getById;
        _create  = create;
        _update  = update;
        _delete  = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getById.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return Created($"/api/v1/sales-orders/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateSalesOrderRequestDto request, CancellationToken ct)
        => Ok(await _update.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
