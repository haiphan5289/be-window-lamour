using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class GetDepositDeductionByIdUseCase : IGetDepositDeductionByIdUseCase
{
    private readonly IDepositDeductionRepository _repo;
    private readonly ILogger<GetDepositDeductionByIdUseCase> _logger;

    public GetDepositDeductionByIdUseCase(IDepositDeductionRepository repo, ILogger<GetDepositDeductionByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<DepositDeductionResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposit deduction {Id}", id);
        var deduction = await _repo.GetByIdAsync(id, ct);
        return deduction is null ? null : GetDepositDeductionsUseCase.MapToDto(deduction);
    }
}
