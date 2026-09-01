using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class UpdateReceiptUseCase : IUpdateReceiptUseCase
{
    private readonly IReceiptRepository     _repo;
    private readonly ILogger<UpdateReceiptUseCase> _logger;

    public UpdateReceiptUseCase(
        IReceiptRepository repo,
        ILogger<UpdateReceiptUseCase> logger)
    {
        _repo     = repo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        int id, UpdateReceiptRequestDto request, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (receipt.Status != ReceiptStatus.Draft)
            throw new DomainException("Chỉ chứng từ ở trạng thái Nháp mới được sửa. Bỏ ghi trước khi sửa.");

        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'.");

        // Update header fields
        receipt.CustomerId          = request.CustomerId;
        receipt.PayerName           = request.PayerName;
        receipt.Address             = request.Address;
        receipt.PaymentReason       = paymentReason;
        receipt.CollectorEmployeeId = request.CollectorEmployeeId;
        receipt.Attachment          = request.Attachment;
        receipt.Reference           = request.Reference;
        receipt.AccountingDate      = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        receipt.DocumentDate        = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc);
        receipt.DocumentNumber      = request.DocumentNumber;

        // Replace entries
        receipt.Entries.Clear();
        foreach (var e in request.Entries)
        {
            if (!Enum.TryParse<AccountCode>(e.DebitAccount, out var debit))
                throw new DomainException($"Invalid debit_account '{e.DebitAccount}'.");
            if (!Enum.TryParse<AccountCode>(e.CreditAccount, out var credit))
                throw new DomainException($"Invalid credit_account '{e.CreditAccount}'.");

            receipt.Entries.Add(new ReceiptEntry
            {
                Description   = e.Description,
                DebitAccount  = debit,
                CreditAccount = credit,
                Amount        = e.Amount,
                SubjectCode   = e.SubjectCode,
                SubjectName   = e.SubjectName,
                BankAccount   = e.BankAccount,
                SalesOrderId  = e.SalesOrderId,
            });
        }

        await _repo.UpdateAsync(receipt, ct);

        // Draft receipt never had a CashTransaction — nothing to sync here anymore. Cash-ledger
        // posting now happens only on Confirm ("Ghi sổ"). See ConfirmReceiptUseCase.

        _logger.LogInformation("Updated Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);

        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
