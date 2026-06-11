using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class GetSalesOrdersUseCase : IGetSalesOrdersUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<GetSalesOrdersUseCase> _logger;

    public GetSalesOrdersUseCase(ISalesOrderRepository repo, ILogger<GetSalesOrdersUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<SalesOrderResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all sales orders");
        var orders = await _repo.GetAllAsync(ct);
        return orders.Select(MapToDto);
    }

    internal static SalesOrderResponseDto MapToDto(Domain.Entities.SalesOrder o) => new()
    {
        Id             = o.Id,
        DocumentNumber = o.DocumentNumber,
        AccountingDate = o.AccountingDate,
        DocumentDate   = o.DocumentDate,
        CustomerId     = o.CustomerId,
        CustomerName   = o.Customer?.Name ?? "",
        EmployeeId     = o.EmployeeId,
        EmployeeName   = o.Employee?.Name,
        Description    = o.Description,
        Reference      = o.Reference,
        PaymentTerms   = o.PaymentTerms,
        PaymentDueDays = o.PaymentDueDays,
        PaymentDueDate = o.PaymentDueDate,
        Notes          = o.Notes,
        DeliveryMethod = o.DeliveryMethod,
        PaymentMethod  = o.PaymentMethod,
        TotalAmount    = o.TotalAmount,
        CreatedAt      = o.CreatedAt,
        Status         = (int)o.Status,
        Lines          = o.Lines.Select(l => new SalesOrderLineDto
        {
            Id                = l.Id,
            ProductId         = l.ProductId,
            ProductCode       = l.ProductCode,
            ProductName       = l.ProductName,
            IsPromotion       = l.IsPromotion,
            Unit              = l.Unit,
            Quantity          = l.Quantity,
            UnitPrice         = l.UnitPrice,
            DiscountRate      = l.DiscountRate,
            Amount            = l.Amount,
            ReceivableAccount = l.ReceivableAccount,
            RevenueAccount    = l.RevenueAccount,
        }).ToList(),
    };
}
