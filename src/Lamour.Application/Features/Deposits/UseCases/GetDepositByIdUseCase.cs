using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetDepositByIdUseCase : IGetDepositByIdUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<GetDepositByIdUseCase> _logger;

    public GetDepositByIdUseCase(IDepositRepository repo, ILogger<GetDepositByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<DepositResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposit {Id}", id);
        var deposit = await _repo.GetByIdAsync(id, ct);
        return deposit is null ? null : GetDepositsUseCase.MapToDto(deposit);
    }
}
