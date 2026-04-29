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
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<UpdateReceiptUseCase> _logger;

    public UpdateReceiptUseCase(
        IReceiptRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<UpdateReceiptUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        int id, UpdateReceiptRequestDto request, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Receipt with id {id} not found.");

        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'.");

        var oldDocumentNumber = receipt.DocumentNumber;

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
            });
        }

        await _repo.UpdateAsync(receipt, ct);

        // Sync CashTransaction: delete old, create new with updated data
        if (!string.IsNullOrWhiteSpace(oldDocumentNumber))
            await _cashRepo.DeleteByReceiptNumberAsync(oldDocumentNumber, ct);

        var totalAmount    = receipt.Entries.Sum(e => e.Amount);
        var counterAccount = receipt.Entries.Count > 0
            ? CreateReceiptUseCase.MapAccountCodeToString(receipt.Entries.First().CreditAccount)
            : "131";

        await _cashRepo.AddAsync(new CashTransaction
        {
            AccountingDate = receipt.AccountingDate,
            DocumentDate   = receipt.DocumentDate,
            ReceiptNumber  = receipt.DocumentNumber,
            PaymentNumber  = null,
            Description    = receipt.PayerName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = totalAmount,
            CreditAmount   = 0m,
            PersonName     = receipt.PayerName,
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        _logger.LogInformation("Updated Receipt {Id} ({DocumentNumber})", id, receipt.DocumentNumber);

        return GetReceiptsUseCase.MapToDto(receipt);
    }
}
