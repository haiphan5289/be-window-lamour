using Lamour.Application.Abstractions;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class DeleteDepositDeductionUseCase : IDeleteDepositDeductionUseCase
{
    private readonly IDepositDeductionRepository _deductionRepo;
    private readonly IDepositRepository          _depositRepo;
    private readonly IUnitOfWork                 _uow;
    private readonly ILogger<DeleteDepositDeductionUseCase> _logger;

    public DeleteDepositDeductionUseCase(
        IDepositDeductionRepository deductionRepo,
        IDepositRepository depositRepo,
        IUnitOfWork uow,
        ILogger<DeleteDepositDeductionUseCase> logger)
    {
        _deductionRepo = deductionRepo;
        _depositRepo   = depositRepo;
        _uow           = uow;
        _logger        = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var deduction = await _deductionRepo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Deposit deduction {id} not found.");

        var deposit = await _depositRepo.GetByIdTrackedAsync(deduction.DepositId, ct)
            ?? throw new NotFoundException($"Deposit {deduction.DepositId} not found.");

        await _uow.BeginAsync(ct);
        try
        {
            deposit.RemainingBalance += deduction.Amount;
            deposit.Status = DepositStatus.Active;
            await _depositRepo.UpdateAsync(deposit, ct);

            await _deductionRepo.DeleteAsync(deduction, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Deleted DepositDeduction {Id} ({DocumentNumber}), restored balance to deposit {DepositId}",
                id, deduction.DocumentNumber, deposit.Id);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
