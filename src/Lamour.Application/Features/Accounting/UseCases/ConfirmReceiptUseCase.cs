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
            throw new DomainException("Chứng từ này đã được ghi sổ.");

        // Cùng field-mapping với dòng Treo ở GetCashLedgerUseCase — dòng không đổi nội dung sau khi ghi sổ.
        var totalAmount    = receipt.Entries.Sum(e => e.Amount);
        var first          = receipt.Entries.FirstOrDefault();
        var counterAccount = first?.CreditAccountSetting?.Code ?? "131";
        // Account theo TK Nợ thực tế của dòng đầu (1111 → 111, 1121 → 112).
        var account        = ReceiptEntryBuilder.LedgerAccount(first?.DebitAccountSetting?.Code);

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
            DocumentType   = ReceiptEntryBuilder.DocumentType(receipt),
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        receipt.Status      = ReceiptStatus.Confirmed;
        receipt.ConfirmedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(receipt, ct);

        _logger.LogInformation("Confirmed Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);

        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
