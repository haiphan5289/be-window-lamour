using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class UpdatePaymentUseCase : IUpdatePaymentUseCase
{
    private readonly IPaymentRepository     _repo;
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<UpdatePaymentUseCase> _logger;

    public UpdatePaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<UpdatePaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(
        int id, UpdatePaymentRequestDto request, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'.");

        var oldDocumentNumber = payment.DocumentNumber;

        // Update header fields
        payment.SupplierId        = request.SupplierId;
        payment.PayeeName         = request.PayeeName;
        payment.Address           = request.Address;
        payment.PaymentReason     = paymentReason;
        payment.PaymentEmployeeId = request.PaymentEmployeeId;
        payment.Attachment        = request.Attachment;
        payment.Reference         = request.Reference;
        payment.AccountingDate    = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        payment.DocumentDate      = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc);
        payment.DocumentNumber    = request.DocumentNumber;

        // Replace entries
        payment.Entries.Clear();
        foreach (var e in request.Entries)
        {
            if (!Enum.TryParse<AccountCode>(e.DebitAccount, out var debit))
                throw new DomainException($"Invalid debit_account '{e.DebitAccount}'.");
            if (!Enum.TryParse<AccountCode>(e.CreditAccount, out var credit))
                throw new DomainException($"Invalid credit_account '{e.CreditAccount}'.");

            payment.Entries.Add(new PaymentEntry
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

        await _repo.UpdateAsync(payment, ct);

        // Sync CashTransaction: delete old, create new with updated data
        if (!string.IsNullOrWhiteSpace(oldDocumentNumber))
            await _cashRepo.DeleteByPaymentNumberAsync(oldDocumentNumber, ct);

        var totalAmount    = payment.Entries.Sum(e => e.Amount);
        var counterAccount = payment.Entries.Count > 0
            ? CreatePaymentUseCase.MapAccountCodeToString(payment.Entries.First().DebitAccount)
            : "131";

        await _cashRepo.AddAsync(new CashTransaction
        {
            AccountingDate = payment.AccountingDate,
            DocumentDate   = payment.DocumentDate,
            ReceiptNumber  = null,
            PaymentNumber  = payment.DocumentNumber,
            Description    = payment.PayeeName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = 0m,
            CreditAmount   = totalAmount,  // Payment decreases cash
            PersonName     = payment.PayeeName,
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        _logger.LogInformation("Updated Payment {Id} ({DocumentNumber})", id, payment.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(payment);
    }
}
