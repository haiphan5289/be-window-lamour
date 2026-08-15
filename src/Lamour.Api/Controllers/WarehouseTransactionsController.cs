using Lamour.Application.Features.Warehouse.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse-transactions")]
[Authorize]
public class WarehouseTransactionsController : ControllerBase
{
    private readonly IGetWarehouseTransactionsUseCase _getTransactions;

    public WarehouseTransactionsController(IGetWarehouseTransactionsUseCase getTransactions)
        => _getTransactions = getTransactions;

    // ?from_date=&to_date=&type=import|export (bỏ trống hoặc "all" = cả hai)
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "from_date")] DateTime? fromDate,
        [FromQuery(Name = "to_date")] DateTime? toDate,
        [FromQuery(Name = "type")] string? type,
        CancellationToken ct)
    {
        var result = await _getTransactions.ExecuteAsync(fromDate, toDate, type, ct);
        return Ok(result);
    }
}
