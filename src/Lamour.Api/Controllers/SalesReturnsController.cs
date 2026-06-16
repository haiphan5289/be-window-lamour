using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.UseCases;
// TODO: [Authorize] — add back when WPF client wires up Bearer token for this endpoint
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/sales-returns")]
public class SalesReturnsController : ControllerBase
{
    private readonly IGetSalesReturnsUseCase        _getAll;
    private readonly IGetSalesReturnByIdUseCase     _getById;
    private readonly IGetNextSalesReturnCodeUseCase _getNextCode;
    private readonly ICreateSalesReturnUseCase      _create;
    private readonly IUpdateSalesReturnUseCase      _update;
    private readonly IDeleteSalesReturnUseCase      _delete;

    public SalesReturnsController(
        IGetSalesReturnsUseCase        getAll,
        IGetSalesReturnByIdUseCase     getById,
        IGetNextSalesReturnCodeUseCase getNextCode,
        ICreateSalesReturnUseCase      create,
        IUpdateSalesReturnUseCase      update,
        IDeleteSalesReturnUseCase      delete)
    {
        _getAll      = getAll;
        _getById     = getById;
        _getNextCode = getNextCode;
        _create      = create;
        _update      = update;
        _delete      = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(ct));

    [HttpGet("next-code")]
    public async Task<IActionResult> GetNextCode(CancellationToken ct)
        => Ok(new { code = await _getNextCode.ExecuteAsync(ct) });

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getById.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesReturnRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return Created($"/api/v1/sales-returns/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateSalesReturnRequestDto request, CancellationToken ct)
        => Ok(await _update.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
