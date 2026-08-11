using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DuplicatePaymentUseCase : IDuplicatePaymentUseCase
{
    private readonly IPaymentRepository _repo;
    private readonly ILogger<DuplicatePaymentUseCase> _logger;

    public DuplicatePaymentUseCase(
        IPaymentRepository repo,
        ILogger<DuplicatePaymentUseCase> logger)
    {
        _repo     = repo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        // Duplicate always starts as a fresh Draft, regardless of the source's status.
        var duplicate = new Payment
        {
            SupplierId        = source.SupplierId,
            PayeeName         = source.PayeeName,
            Address           = source.Address,
            PaymentReason     = source.PaymentReason,
            ReasonDetail      = source.ReasonDetail,
            PaymentEmployeeId = source.PaymentEmployeeId,
            Attachment        = source.Attachment,
            Reference         = source.Reference,
            AccountingDate    = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            DocumentDate      = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            DocumentNumber    = $"{source.DocumentNumber}-COPY",
            Status            = PaymentStatus.Draft,
            CreatedAt         = DateTime.UtcNow,
            Entries           = source.Entries.Select(e => new PaymentEntry
            {
                Description             = e.Description,
                DebitAccountSettingId    = e.DebitAccountSettingId,
                CreditAccountSettingId   = e.CreditAccountSettingId,
                Amount                   = e.Amount,
                SubjectCode              = e.SubjectCode,
                SubjectName              = e.SubjectName,
                BankAccount              = e.BankAccount,
                ExpenseCategoryId        = e.ExpenseCategoryId,
            }).ToList(),
        };

        var saved = await _repo.AddAsync(duplicate, ct);

        _logger.LogInformation("Duplicated Payment {SourceId} → {NewId} ({DocumentNumber}) as Draft",
            id, saved.Id, saved.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(saved);
    }
}
