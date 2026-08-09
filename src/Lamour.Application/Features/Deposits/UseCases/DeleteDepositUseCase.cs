using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class DeleteDepositUseCase : IDeleteDepositUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<DeleteDepositUseCase> _logger;

    public DeleteDepositUseCase(IDepositRepository repo, ILogger<DeleteDepositUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var deposit = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Deposit {id} not found.");

        if (deposit.RemainingBalance != deposit.Amount)
            throw new DomainException("Cọc đã bị trừ, không thể xóa.");

        await _repo.DeleteAsync(deposit, ct);

        _logger.LogInformation("Deleted Deposit {Id} ({DocumentNumber})", id, deposit.DocumentNumber);
    }
}
