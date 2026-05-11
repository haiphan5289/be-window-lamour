using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DuplicatePaymentUseCase : IDuplicatePaymentUseCase
{
    private readonly IPaymentRepository _repo;
    private readonly ICashLedgerRepository _cashRepo;
    private readonly ILogger<DuplicatePaymentUseCase> _logger;

    public DuplicatePaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<DuplicatePaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        // Clone payment with new document number
        var duplicate = new Payment
        {
            SupplierId        = source.SupplierId,
            PayeeName         = source.PayeeName,
            Address           = source.Address,
            PaymentReason     = source.PaymentReason,
            PaymentEmployeeId = source.PaymentEmployeeId,
            Attachment        = source.Attachment,
            Reference         = source.Reference,
            AccountingDate    = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            DocumentDate      = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            DocumentNumber    = $"{source.DocumentNumber}-COPY",
            CreatedAt         = DateTime.UtcNow,
            Entries           = source.Entries.Select(e => new PaymentEntry
            {
                Description   = e.Description,
                DebitAccount  = e.DebitAccount,
                CreditAccount = e.CreditAccount,
                Amount        = e.Amount,
                SubjectCode   = e.SubjectCode,
                SubjectName   = e.SubjectName,
                BankAccount   = e.BankAccount,
            }).ToList(),
        };

        var saved = await _repo.AddAsync(duplicate, ct);

        // Auto-create CashTransaction for duplicate
        var totalAmount  = saved.Entries.Sum(e => e.Amount);
        var counterAccount = saved.Entries.Count > 0
            ? CreatePaymentUseCase.MapAccountCodeToString(saved.Entries.First().DebitAccount)
            : "131";

        await _cashRepo.AddAsync(new CashTransaction
        {
            AccountingDate = saved.AccountingDate,
            DocumentDate   = saved.DocumentDate,
            ReceiptNumber  = null,
            PaymentNumber  = saved.DocumentNumber,
            Description    = saved.PayeeName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = 0m,
            CreditAmount   = totalAmount,
            PersonName     = saved.PayeeName,
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        _logger.LogInformation("Duplicated Payment {SourceId} → {NewId} ({DocumentNumber})",
            id, saved.Id, saved.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(saved);
    }
}
