using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetDepositDeductionsUseCase : IGetDepositDeductionsUseCase
{
    private readonly IDepositDeductionRepository _repo;
    private readonly ILogger<GetDepositDeductionsUseCase> _logger;

    public GetDepositDeductionsUseCase(IDepositDeductionRepository repo, ILogger<GetDepositDeductionsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposit deduction report");
        var deductions = await _repo.GetAllAsync(customerId, employeeId, salesOrderId, fromDate, toDate, ct);
        return deductions.Select(MapToDto);
    }

    internal static DepositDeductionResponseDto MapToDto(Domain.Entities.DepositDeduction x) => new()
    {
        Id                       = x.Id,
        DocumentNumber           = x.DocumentNumber,
        DepositId                = x.DepositId,
        DepositDocumentNumber    = x.Deposit?.DocumentNumber ?? "",
        SalesOrderId             = x.SalesOrderId,
        SalesOrderDocumentNumber = x.SalesOrder?.DocumentNumber ?? "",
        CustomerId               = x.Deposit?.CustomerId ?? 0,
        CustomerName             = x.Deposit?.Customer?.Name ?? "",
        EmployeeId               = x.SalesOrder?.EmployeeId,
        EmployeeName             = x.SalesOrder?.Employee?.Name,
        Amount                   = x.Amount,
        AccountingDate           = x.AccountingDate,
        DocumentDate             = x.DocumentDate,
        Description              = x.Description,
        CreatedAt                = x.CreatedAt,
    };
}
