using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DeletePaymentUseCase : IDeletePaymentUseCase
{
    private readonly IPaymentRepository     _repo;
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<DeletePaymentUseCase> _logger;

    public DeletePaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<DeletePaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        // Delete associated CashTransaction by DocumentNumber
        if (!string.IsNullOrWhiteSpace(payment.DocumentNumber))
            await _cashRepo.DeleteByPaymentNumberAsync(payment.DocumentNumber, ct);

        await _repo.DeleteAsync(payment, ct);

        _logger.LogInformation("Deleted Payment {Id} ({DocumentNumber})", id, payment.DocumentNumber);
    }
}
