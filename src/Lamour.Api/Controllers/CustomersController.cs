using Lamour.Application.Features.Customers.Dtos;
using Lamour.Application.Features.Customers.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
// TODO: restore [Authorize] once WPF auth flow is wired up
public class CustomersController : ControllerBase
{
    private readonly IGetCustomersUseCase      _getAll;
    private readonly IGetNextCustomerCodeUseCase _nextCode;
    private readonly ICreateCustomerUseCase    _create;
    private readonly IUpdateCustomerUseCase    _update;
    private readonly IDeleteCustomerUseCase    _delete;
    private readonly IDuplicateCustomerUseCase _duplicate;

    public CustomersController(
        IGetCustomersUseCase      getAll,
        IGetNextCustomerCodeUseCase nextCode,
        ICreateCustomerUseCase    create,
        IUpdateCustomerUseCase    update,
        IDeleteCustomerUseCase    delete,
        IDuplicateCustomerUseCase duplicate)
    {
        _getAll    = getAll;
        _nextCode  = nextCode;
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

    [HttpGet("next-code")]
    public async Task<IActionResult> NextCode(CancellationToken ct)
    {
        var code = await _nextCode.ExecuteAsync(ct);
        return Ok(new { code });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequestDto request, CancellationToken ct)
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
