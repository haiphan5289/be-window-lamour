using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetDepositsByCustomerUseCase : IGetDepositsByCustomerUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<GetDepositsByCustomerUseCase> _logger;

    public GetDepositsByCustomerUseCase(IDepositRepository repo, ILogger<GetDepositsByCustomerUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<DepositResponseDto>> ExecuteAsync(int customerId, int? excludeSalesOrderId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposits with remaining balance for customer {CustomerId}", customerId);
        var deposits = await _repo.GetByCustomerIdAsync(customerId, excludeSalesOrderId, ct);
        return deposits.Select(GetDepositsUseCase.MapToDto);
    }
}
