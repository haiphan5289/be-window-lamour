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
    private readonly IGetInventoryDetailByProductUseCase _getDetail;

    public InventoryController(
        IGetInventorySummaryUseCase getSummary,
        IGetInventoryDetailByProductUseCase getDetail)
    {
        _getSummary = getSummary;
        _getDetail  = getDetail;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateOnly from_date,
        [FromQuery] DateOnly to_date,
        [FromQuery] int[]? warehouse_ids,
        [FromQuery] int? category_id,
        [FromQuery] int? product_unit_id,
        [FromQuery] int[]? product_ids,
        CancellationToken ct)
    {
        var result = await _getSummary.ExecuteAsync(from_date, to_date, warehouse_ids, category_id, product_unit_id, product_ids, ct);
        return Ok(result);
    }

    // Drill-down "Sổ chi tiết vật tư hàng hóa" cho 1 sản phẩm — double-click 1 dòng ở Tổng hợp tồn kho.
    [HttpGet("summary/{productId:int}/detail")]
    public async Task<IActionResult> GetDetail(
        int productId,
        [FromQuery] DateOnly from_date,
        [FromQuery] DateOnly to_date,
        [FromQuery] int[]? warehouse_ids,
        CancellationToken ct)
    {
        var result = await _getDetail.ExecuteAsync(productId, from_date, to_date, warehouse_ids, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
