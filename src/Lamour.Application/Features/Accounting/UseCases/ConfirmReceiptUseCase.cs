using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// "Ghi sổ" — chuyển Receipt từ Draft sang Confirmed, tại đây mới post CashTransaction (cash-ledger
// side-effect). Mirror ConfirmPaymentUseCase (post CashTransaction on Confirm) + ConfirmSalesReturnUseCase
// (2-state Draft/Confirmed, no "Treo" state).
public class ConfirmReceiptUseCase : IConfirmReceiptUseCase
{
    private readonly IReceiptRepository    _repo;
    private readonly ICashLedgerRepository _cashRepo;
    private readonly ILogger<ConfirmReceiptUseCase> _logger;

    public ConfirmReceiptUseCase(
        IReceiptRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<ConfirmReceiptUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (receipt.Status != ReceiptStatus.Draft)
            throw new DomainException("Chỉ chứng từ ở trạng thái Nháp mới có thể ghi sổ.");

        // Cùng field-mapping logic đã có ở CreateReceiptUseCase (trước khi bị gỡ khỏi Create) — chỉ
        // chuyển thời điểm thực thi từ Create sang đây.
        var totalAmount  = receipt.Entries.Sum(e => e.Amount);
        var counterAccount = receipt.Entries.Count > 0
            ? CreateReceiptUseCase.MapAccountCodeToString(receipt.Entries.First().CreditAccount)
            : "131";
        // Account theo TK Nợ thực tế của dòng đầu (Cash111/Bank112) — trước đây hardcode "111" nên
        // phiếu thu chọn Bank112 vẫn bị ghi nhầm vào sổ quỹ tiền mặt thay vì tiền gửi ngân hàng.
        var account = receipt.Entries.Count > 0
            ? CreateReceiptUseCase.MapAccountCodeToString(receipt.Entries.First().DebitAccount)
            : "111";

        await _cashRepo.AddAsync(new CashTransaction
        {
            AccountingDate = receipt.AccountingDate,
            DocumentDate   = receipt.DocumentDate,
            ReceiptNumber  = receipt.DocumentNumber,
            PaymentNumber  = null,
            Description    = receipt.PayerName,
            Account        = account,
            CounterAccount = counterAccount,
            DebitAmount    = totalAmount,
            CreditAmount   = 0m,
            PersonName     = receipt.PayerName,
            PaymentReason  = receipt.PaymentReason.ToString(),
            DocumentType   = receipt.CustomerId is null
                ? "Phiếu thu tiền mặt khách hàng hàng loạt"
                : "Phiếu thu tiền mặt khách hàng",
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        receipt.Status      = ReceiptStatus.Confirmed;
        receipt.ConfirmedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(receipt, ct);

        _logger.LogInformation("Confirmed Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);

        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
