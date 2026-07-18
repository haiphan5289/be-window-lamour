using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.SalesReturn.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetSalesOrderSummaryReportUseCase
{
    Task<IEnumerable<SalesOrderSummaryLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}

public class GetSalesOrderSummaryReportUseCase : IGetSalesOrderSummaryReportUseCase
{
    private readonly ISalesOrderRepository _salesRepo;
    private readonly ISalesReturnRepository _returnRepo;
    private readonly ILogger<GetSalesOrderSummaryReportUseCase> _logger;

    public GetSalesOrderSummaryReportUseCase(
        ISalesOrderRepository salesRepo,
        ISalesReturnRepository returnRepo,
        ILogger<GetSalesOrderSummaryReportUseCase> logger)
    {
        _salesRepo  = salesRepo;
        _returnRepo = returnRepo;
        _logger     = logger;
    }

    public async Task<IEnumerable<SalesOrderSummaryLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Fetching sales order summary report (employeeId={EmployeeId}, customerId={CustomerId}, unit={Unit}, category={Category}, from={From}, to={To})",
            employeeId, customerId, unit, category, fromDate, toDate);

        var salesLines  = await _salesRepo.GetReportLinesAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);
        var returnLines = await _returnRepo.GetReportLinesAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);

        var map = new Dictionary<(int ProductId, int CustomerId, int? EmployeeId), SalesOrderSummaryLineDto>();

        SalesOrderSummaryLineDto GetOrAdd(
            int productId, string productCode, string productName, string unitName,
            int customerId, string customerCode, string customerName,
            int? employeeId, string? employeeCode, string? employeeName)
        {
            var key = (productId, customerId, employeeId);
            if (!map.TryGetValue(key, out var dto))
            {
                dto = new SalesOrderSummaryLineDto
                {
                    ProductId    = productId,
                    ProductCode  = productCode,
                    ProductName  = productName,
                    Unit         = unitName,
                    CustomerId   = customerId,
                    CustomerCode = customerCode,
                    CustomerName = customerName,
                    EmployeeId   = employeeId,
                    EmployeeCode = employeeCode,
                    EmployeeName = employeeName,
                };
                map[key] = dto;
            }
            return dto;
        }

        foreach (var l in salesLines)
        {
            var dto = GetOrAdd(
                l.ProductId, l.ProductCode, l.ProductName, l.Unit,
                l.SalesOrder.CustomerId, l.SalesOrder.Customer?.Code ?? "", l.SalesOrder.Customer?.Name ?? "",
                l.SalesOrder.EmployeeId, l.SalesOrder.Employee?.Code, l.SalesOrder.Employee?.Name);

            dto.QuantitySold   += l.Quantity;
            dto.SalesAmount    += l.Quantity * l.UnitPrice;
            dto.DiscountAmount += l.Quantity * l.UnitPrice * l.DiscountRate / 100m;
        }

        foreach (var l in returnLines)
        {
            var dto = GetOrAdd(
                l.ProductId, l.ProductCode, l.ProductName, l.Unit,
                l.SalesReturn.CustomerId, l.SalesReturn.Customer?.Code ?? "", l.SalesReturn.Customer?.Name ?? "",
                l.SalesReturn.EmployeeId, l.SalesReturn.Employee?.Code, l.SalesReturn.Employee?.Name);

            dto.ReturnQuantity += l.Quantity;
            dto.ReturnValue    += l.Amount - l.DiscountAmount;
        }

        foreach (var dto in map.Values)
            dto.NetRevenue = dto.SalesAmount - dto.DiscountAmount - dto.ReturnValue;

        return map.Values;
    }
}
