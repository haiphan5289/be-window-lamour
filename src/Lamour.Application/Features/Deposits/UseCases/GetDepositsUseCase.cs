using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetDepositsUseCase : IGetDepositsUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<GetDepositsUseCase> _logger;

    public GetDepositsUseCase(IDepositRepository repo, ILogger<GetDepositsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<DepositResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all deposits");
        var deposits = await _repo.GetAllAsync(ct);
        return deposits.Select(MapToDto);
    }

    internal static DepositResponseDto MapToDto(Domain.Entities.Deposit d) => new()
    {
        Id               = d.Id,
        DocumentNumber   = d.DocumentNumber,
        AccountingDate   = d.AccountingDate,
        DocumentDate     = d.DocumentDate,
        CustomerId       = d.CustomerId,
        CustomerName     = d.Customer?.Name ?? "",
        EmployeeId       = d.EmployeeId,
        EmployeeName     = d.Employee?.Name,
        Description      = d.Description,
        Reference        = d.Reference,
        Amount           = d.Amount,
        RemainingBalance = d.RemainingBalance,
        Status           = (int)d.Status,
        CreatedAt        = d.CreatedAt,
        Deductions       = d.Deductions.Select(GetDepositDeductionsUseCase.MapToDto).ToList(),
    };
}
