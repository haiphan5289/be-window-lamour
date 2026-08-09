using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/deposits")]
[Authorize]
public class DepositsController : ControllerBase
{
    private readonly IGetDepositsUseCase           _getAll;
    private readonly IGetDepositByIdUseCase        _getById;
    private readonly IGetNextDepositCodeUseCase    _getNextCode;
    private readonly IGetDepositsByCustomerUseCase _getByCustomer;
    private readonly ICreateDepositUseCase         _create;
    private readonly IUpdateDepositUseCase         _update;
    private readonly IDeleteDepositUseCase         _delete;

    public DepositsController(
        IGetDepositsUseCase           getAll,
        IGetDepositByIdUseCase        getById,
        IGetNextDepositCodeUseCase    getNextCode,
        IGetDepositsByCustomerUseCase getByCustomer,
        ICreateDepositUseCase         create,
        IUpdateDepositUseCase         update,
        IDeleteDepositUseCase         delete)
    {
        _getAll        = getAll;
        _getById       = getById;
        _getNextCode   = getNextCode;
        _getByCustomer = getByCustomer;
        _create        = create;
        _update        = update;
        _delete        = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(ct));

    [HttpGet("next-code")]
    public async Task<IActionResult> GetNextCode(CancellationToken ct)
        => Ok(new { code = await _getNextCode.ExecuteAsync(ct) });

    [HttpGet("by-customer/{customerId:int}")]
    public async Task<IActionResult> GetByCustomer(int customerId, CancellationToken ct)
        => Ok(await _getByCustomer.ExecuteAsync(customerId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getById.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepositRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return Created($"/api/v1/deposits/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepositRequestDto request, CancellationToken ct)
        => Ok(await _update.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
