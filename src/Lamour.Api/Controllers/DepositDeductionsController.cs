using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/deposit-deductions")]
[Authorize]
public class DepositDeductionsController : ControllerBase
{
    private readonly IGetDepositDeductionsUseCase  _getAll;
    private readonly IGetDepositDeductionByIdUseCase _getById;
    private readonly ICreateDepositDeductionUseCase _create;
    private readonly IDeleteDepositDeductionUseCase _delete;

    public DepositDeductionsController(
        IGetDepositDeductionsUseCase   getAll,
        IGetDepositDeductionByIdUseCase getById,
        ICreateDepositDeductionUseCase create,
        IDeleteDepositDeductionUseCase delete)
    {
        _getAll  = getAll;
        _getById = getById;
        _create  = create;
        _delete  = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? customer_id,
        [FromQuery] int? employee_id,
        [FromQuery] int? sales_order_id,
        [FromQuery] DateTime? from_date,
        [FromQuery] DateTime? to_date,
        CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(customer_id, employee_id, sales_order_id, from_date, to_date, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _getById.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepositDeductionRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return Created($"/api/v1/deposit-deductions/{result.Id}", result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
