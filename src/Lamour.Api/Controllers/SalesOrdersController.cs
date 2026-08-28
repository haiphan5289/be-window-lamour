using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.UseCases;
using Lamour.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly IGetSalesOrdersUseCase         _getAll;
    private readonly IGetSalesOrderByIdUseCase      _getById;
    private readonly IGetNextSalesOrderCodeUseCase  _getNextCode;
    private readonly ICreateSalesOrderUseCase       _create;
    private readonly IUpdateSalesOrderUseCase       _update;
    private readonly IDeleteSalesOrderUseCase       _delete;
    private readonly IHoldSalesOrderUseCase         _hold;
    private readonly IGetSalesOrderReportUseCase    _report;
    private readonly IGetSalesOrderSummaryReportUseCase _summaryReport;
    private readonly IDuplicateSalesOrderUseCase    _duplicate;

    public SalesOrdersController(
        IGetSalesOrdersUseCase        getAll,
        IGetSalesOrderByIdUseCase     getById,
        IGetNextSalesOrderCodeUseCase getNextCode,
        ICreateSalesOrderUseCase      create,
        IUpdateSalesOrderUseCase      update,
        IDeleteSalesOrderUseCase      delete,
        IHoldSalesOrderUseCase        hold,
        IGetSalesOrderReportUseCase   report,
        IGetSalesOrderSummaryReportUseCase summaryReport,
        IDuplicateSalesOrderUseCase   duplicate)
    {
        _getAll        = getAll;
        _getById       = getById;
        _getNextCode   = getNextCode;
        _create        = create;
        _update        = update;
        _delete        = delete;
        _hold          = hold;
        _report        = report;
        _summaryReport = summaryReport;
        _duplicate     = duplicate;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from_date, [FromQuery] DateTime? to_date, [FromQuery] string? search, CancellationToken ct)
        => Ok(await _getAll.ExecuteAsync(from_date, to_date, search, ct));

    // source=direct → BH (mở từ "Bán hàng"); mặc định/bất kỳ giá trị khác (kể cả không truyền,
    // giữ tương thích ngược với client cũ) → XK (mở từ "Xuất kho bán hàng").
    [HttpGet("next-code")]
    public async Task<IActionResult> GetNextCode([FromQuery] string? source, CancellationToken ct)
    {
        var codeSource = string.Equals(source, "direct", StringComparison.OrdinalIgnoreCase)
            ? SalesOrderCodeSource.Direct
            : SalesOrderCodeSource.WarehouseExport;
        return Ok(new { code = await _getNextCode.ExecuteAsync(codeSource, ct) });
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetReport(
        [FromQuery] int[]? product_ids,
        [FromQuery] int? employee_id,
        [FromQuery] int? customer_id,
        [FromQuery] string? unit,
        [FromQuery] string? category,
        [FromQuery] DateTime? from_date,
        [FromQuery] DateTime? to_date,
        CancellationToken ct)
        => Ok(await _report.ExecuteAsync(product_ids, employee_id, customer_id, unit, category, from_date, to_date, ct));

    [HttpGet("summary-report")]
    public async Task<IActionResult> GetSummaryReport(
        [FromQuery] int[]? product_ids,
        [FromQuery] int? employee_id,
        [FromQuery] int? customer_id,
        [FromQuery] string? unit,
        [FromQuery] string? category,
        [FromQuery] DateTime? from_date,
        [FromQuery] DateTime? to_date,
        CancellationToken ct)
        => Ok(await _summaryReport.ExecuteAsync(product_ids, employee_id, customer_id, unit, category, from_date, to_date, ct));

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

    [HttpPut("{id:int}/hold")]
    public async Task<IActionResult> Hold(int id, CancellationToken ct)
        => Ok(await _hold.ExecuteAsync(id, ct));

    [HttpPost("{id:int}/duplicate")]
    public async Task<IActionResult> Duplicate(int id, CancellationToken ct)
    {
        var result = await _duplicate.ExecuteAsync(id, ct);
        return Created($"/api/v1/sales-orders/{result.Id}", result);
    }
}
