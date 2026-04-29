using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class CreateReceiptUseCase : ICreateReceiptUseCase
{
    private readonly IReceiptRepository     _repo;
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<CreateReceiptUseCase> _logger;

    public CreateReceiptUseCase(
        IReceiptRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<CreateReceiptUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        CreateReceiptRequestDto request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'. Valid values: ThuKhac, ThuTienHang, ThuCongNo.");

        var entries = request.Entries.Select(e =>
        {
            if (!Enum.TryParse<AccountCode>(e.DebitAccount, out var debit))
                throw new DomainException($"Invalid debit_account '{e.DebitAccount}'.");
            if (!Enum.TryParse<AccountCode>(e.CreditAccount, out var credit))
                throw new DomainException($"Invalid credit_account '{e.CreditAccount}'.");
            return new ReceiptEntry
            {
                Description   = e.Description,
                DebitAccount  = debit,
                CreditAccount = credit,
                Amount        = e.Amount,
                SubjectCode   = e.SubjectCode,
                SubjectName   = e.SubjectName,
                BankAccount   = e.BankAccount,
            };
        }).ToList();

        var receipt = new Receipt
        {
            CustomerId          = request.CustomerId,
            PayerName           = request.PayerName,
            Address             = request.Address,
            PaymentReason       = paymentReason,
            CollectorEmployeeId = request.CollectorEmployeeId,
            Attachment          = request.Attachment,
            Reference           = request.Reference,
            AccountingDate      = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate        = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc),
            DocumentNumber      = request.DocumentNumber,
            CreatedAt           = DateTime.UtcNow,
            Entries             = entries,
        };

        var saved = await _repo.AddAsync(receipt, ct);

        // Auto-create CashTransaction
        var totalAmount  = entries.Sum(e => e.Amount);
        var counterAccount = entries.Count > 0
            ? MapAccountCodeToString(entries[0].CreditAccount)
            : "131";

        var cashTx = new CashTransaction
        {
            AccountingDate = saved.AccountingDate,
            DocumentDate   = saved.DocumentDate,
            ReceiptNumber  = saved.DocumentNumber,
            PaymentNumber  = null,
            Description    = saved.PayerName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = totalAmount,
            CreditAmount   = 0m,
            PersonName     = saved.PayerName,
            CreatedAt      = DateTime.UtcNow,
        };

        await _cashRepo.AddAsync(cashTx, ct);

        _logger.LogInformation("Created Receipt {DocumentNumber} for customer {CustomerId}",
            saved.DocumentNumber, saved.CustomerId);

        return GetReceiptsUseCase.MapToDto(saved);
    }

    internal static string MapAccountCodeToString(AccountCode code) => code switch
    {
        AccountCode.Cash111       => "111",
        AccountCode.Bank112       => "112",
        AccountCode.Receivable131 => "131",
        AccountCode.Payroll334    => "334",
        _                         => "131",
    };
}
