using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// "Bỏ ghi" — đưa Receipt đã Confirmed quay lại Draft, xoá CashTransaction đã tạo lúc Confirm.
// Mirror UnconfirmPaymentUseCase.
public class UnconfirmReceiptUseCase : IUnconfirmReceiptUseCase
{
    private readonly IReceiptRepository    _repo;
    private readonly ICashLedgerRepository _cashRepo;
    private readonly ILogger<UnconfirmReceiptUseCase> _logger;

    public UnconfirmReceiptUseCase(
        IReceiptRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<UnconfirmReceiptUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (receipt.Status != ReceiptStatus.Confirmed)
            throw new DomainException("Chỉ chứng từ đã ghi sổ mới có thể bỏ ghi.");

        await _cashRepo.DeleteByReceiptNumberAsync(receipt.DocumentNumber, ct);

        receipt.Status      = ReceiptStatus.Draft;
        receipt.ConfirmedAt = null;
        await _repo.UpdateAsync(receipt, ct);

        _logger.LogInformation("Unconfirmed Receipt {Id} ({DocumentNumber}) — reverted to Draft", id, receipt.DocumentNumber);

        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
