using Lamour.Application.Features.Warehouse.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IGetInventorySummaryUseCase _getSummary;

    public InventoryController(IGetInventorySummaryUseCase getSummary)
        => _getSummary = getSummary;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateOnly from_date,
        [FromQuery] DateOnly to_date,
        [FromQuery] int[]? warehouse_ids,
        [FromQuery] int? category_id,
        [FromQuery] int? product_unit_id,
        CancellationToken ct)
    {
        var result = await _getSummary.ExecuteAsync(from_date, to_date, warehouse_ids, category_id, product_unit_id, ct);
        return Ok(result);
    }
}
