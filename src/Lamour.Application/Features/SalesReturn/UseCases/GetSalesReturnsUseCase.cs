using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

using SalesReturnEntity = Lamour.Domain.Entities.SalesReturn;

public class GetSalesReturnsUseCase : IGetSalesReturnsUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly ILogger<GetSalesReturnsUseCase> _logger;

    public GetSalesReturnsUseCase(ISalesReturnRepository repo, ILogger<GetSalesReturnsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<SalesReturnResponseDto>> ExecuteAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales returns (fromDate={FromDate}, toDate={ToDate}, search={Search})", fromDate, toDate, search);
        var returns = await _repo.GetAllAsync(fromDate, toDate, search, ct);
        return returns.Select(MapToDto);
    }

    internal static SalesReturnResponseDto MapToDto(SalesReturnEntity sr) => new()
    {
        Id             = sr.Id,
        DocumentNumber = sr.DocumentNumber,
        AccountingDate = sr.AccountingDate,
        DocumentDate   = sr.DocumentDate,
        CustomerId     = sr.CustomerId,
        CustomerName   = sr.Customer?.Name ?? "",
        EmployeeId     = sr.EmployeeId,
        EmployeeName   = sr.Employee?.Name,
        Description    = sr.Description,
        Reference      = sr.Reference,
        ReturnType     = (int)sr.ReturnType,
        TotalAmount    = sr.TotalAmount,
        TotalDiscount  = sr.TotalDiscount,
        TotalPayment   = sr.TotalPayment,
        CreatedAt      = sr.CreatedAt,
        Lines          = sr.Lines.Select(l => new SalesReturnLineDto
        {
            Id               = l.Id,
            ProductId        = l.ProductId,
            ProductCode      = l.ProductCode,
            ProductName      = l.ProductName,
            ReturnAccount    = l.ReturnAccount,
            DebtAccount      = l.DebtAccount,
            DiscountAccount  = l.DiscountAccount,
            Unit             = l.Unit,
            Quantity         = l.Quantity,
            UnitPrice        = l.UnitPrice,
            Amount           = l.Amount,
            DiscountRate     = l.DiscountRate,
            DiscountAmount   = l.DiscountAmount,
            SalesOrderNumber = l.SalesOrderNumber,
            TaxRate          = l.TaxRate,
            TaxAmount        = l.TaxAmount,
            TaxAccount       = l.TaxAccount,
            CostAccount      = l.CostAccount,
            CogsAccount      = l.CogsAccount,
            CostPrice        = l.CostPrice,
            CostAmount       = l.CostAmount,
            DepartmentId     = l.DepartmentId,
            DepartmentName   = l.Department?.Name,
        }).ToList(),
    };
}
