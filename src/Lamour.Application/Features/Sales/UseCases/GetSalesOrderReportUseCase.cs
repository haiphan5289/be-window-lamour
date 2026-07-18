using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetSalesOrderReportUseCase
{
    Task<IEnumerable<SalesOrderReportLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}

public class GetSalesOrderReportUseCase : IGetSalesOrderReportUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<GetSalesOrderReportUseCase> _logger;

    public GetSalesOrderReportUseCase(ISalesOrderRepository repo, ILogger<GetSalesOrderReportUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<SalesOrderReportLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Fetching sales order report (employeeId={EmployeeId}, customerId={CustomerId}, unit={Unit}, category={Category}, from={From}, to={To})",
            employeeId, customerId, unit, category, fromDate, toDate);

        var lines = await _repo.GetReportLinesAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);

        return lines.Select(l => new SalesOrderReportLineDto
        {
            OrderId        = l.SalesOrderId,
            DocumentNumber = l.SalesOrder.DocumentNumber,
            AccountingDate = l.SalesOrder.AccountingDate,
            CustomerId     = l.SalesOrder.CustomerId,
            CustomerName   = l.SalesOrder.Customer?.Name ?? "",
            EmployeeId     = l.SalesOrder.EmployeeId,
            EmployeeName   = l.SalesOrder.Employee?.Name,
            ProductId      = l.ProductId,
            ProductCode    = l.ProductCode,
            ProductName    = l.ProductName,
            Unit           = l.Unit,
            Category       = l.Product?.Category,
            Quantity       = l.Quantity,
            UnitPrice      = l.UnitPrice,
            DiscountRate   = l.DiscountRate,
            Amount         = l.Amount,
            TaxRate        = l.TaxRate,
            TaxAmount      = l.TaxAmount,
        });
    }
}
