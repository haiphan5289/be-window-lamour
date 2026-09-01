using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DeleteReceiptUseCase : IDeleteReceiptUseCase
{
    private readonly IReceiptRepository     _repo;
    private readonly ILogger<DeleteReceiptUseCase> _logger;

    public DeleteReceiptUseCase(
        IReceiptRepository repo,
        ILogger<DeleteReceiptUseCase> logger)
    {
        _repo     = repo;
        _logger   = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (receipt.Status != ReceiptStatus.Draft)
            throw new DomainException("Chỉ chứng từ ở trạng thái Nháp mới được xóa. Bỏ ghi trước khi xóa.");

        // Draft receipt never had a CashTransaction — nothing to clean up here anymore.

        await _repo.DeleteAsync(receipt, ct);

        _logger.LogInformation("Deleted Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);
    }
}
