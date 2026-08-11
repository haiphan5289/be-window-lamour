using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetPaymentsUseCase : IGetPaymentsUseCase
{
    private readonly IPaymentRepository _repo;
    private readonly ILogger<GetPaymentsUseCase> _logger;

    public GetPaymentsUseCase(IPaymentRepository repo, ILogger<GetPaymentsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all payments");
        var payments = await _repo.GetAllAsync(ct);
        return payments.Select(MapToDto);
    }

    internal static PaymentResponseDto MapToDto(Domain.Entities.Payment p) => new()
    {
        Id                  = p.Id,
        SupplierId          = p.SupplierId,
        SupplierName        = p.Supplier?.Name ?? "",
        PayeeName           = p.PayeeName,
        Address             = p.Address,
        PaymentReason       = p.PaymentReason.ToString(),
        ReasonDetail        = p.ReasonDetail,
        PaymentEmployeeId   = p.PaymentEmployeeId,
        PaymentEmployeeName = p.PaymentEmployee?.Name,
        Attachment          = p.Attachment,
        Reference           = p.Reference,
        AccountingDate      = p.AccountingDate,
        DocumentDate        = p.DocumentDate,
        DocumentNumber      = p.DocumentNumber,
        Status              = p.Status.ToString(),
        CreatedAt           = p.CreatedAt,
        ConfirmedAt         = p.ConfirmedAt,
        Entries             = p.Entries.Select(e => new PaymentEntryDto
        {
            Id                        = e.Id,
            Description               = e.Description,
            DebitAccountId            = e.DebitAccountSettingId,
            DebitAccountCode          = e.DebitAccountSetting?.Code,
            DebitAccountDescription   = e.DebitAccountSetting?.Description,
            CreditAccountId           = e.CreditAccountSettingId,
            CreditAccountCode         = e.CreditAccountSetting?.Code,
            CreditAccountDescription  = e.CreditAccountSetting?.Description,
            Amount                    = e.Amount,
            SubjectCode               = e.SubjectCode,
            SubjectName               = e.SubjectName,
            BankAccount               = e.BankAccount,
            ExpenseCategoryId         = e.ExpenseCategoryId,
            ExpenseCategoryName       = e.ExpenseCategory?.Name,
        }).ToList(),
    };
}
