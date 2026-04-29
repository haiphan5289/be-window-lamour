using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DeleteReceiptUseCase : IDeleteReceiptUseCase
{
    private readonly IReceiptRepository     _repo;
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<DeleteReceiptUseCase> _logger;

    public DeleteReceiptUseCase(
        IReceiptRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<DeleteReceiptUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        // Delete associated CashTransaction by DocumentNumber
        if (!string.IsNullOrWhiteSpace(receipt.DocumentNumber))
            await _cashRepo.DeleteByReceiptNumberAsync(receipt.DocumentNumber, ct);

        await _repo.DeleteAsync(receipt, ct);

        _logger.LogInformation("Deleted Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);
    }
}
