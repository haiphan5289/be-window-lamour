using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentRepository     _repo;
    private readonly ICashLedgerRepository  _cashRepo;
    private readonly ILogger<CreatePaymentUseCase> _logger;

    public CreatePaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<CreatePaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(
        CreatePaymentRequestDto request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'. Valid values: ChiKhac, ChiMuaHang, ChiTraNo, ChiLuong.");

        var entries = request.Entries.Select(e =>
        {
            if (!Enum.TryParse<AccountCode>(e.DebitAccount, out var debit))
                throw new DomainException($"Invalid debit_account '{e.DebitAccount}'.");
            if (!Enum.TryParse<AccountCode>(e.CreditAccount, out var credit))
                throw new DomainException($"Invalid credit_account '{e.CreditAccount}'.");
            return new PaymentEntry
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

        var payment = new Payment
        {
            SupplierId        = request.SupplierId,
            PayeeName         = request.PayeeName,
            Address           = request.Address,
            PaymentReason     = paymentReason,
            PaymentEmployeeId = request.PaymentEmployeeId,
            Attachment        = request.Attachment,
            Reference         = request.Reference,
            AccountingDate    = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate      = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc),
            DocumentNumber    = request.DocumentNumber,
            CreatedAt         = DateTime.UtcNow,
            Entries           = entries,
        };

        var saved = await _repo.AddAsync(payment, ct);

        // Auto-create CashTransaction (CREDIT - decreases cash)
        var totalAmount  = entries.Sum(e => e.Amount);
        var counterAccount = entries.Count > 0
            ? MapAccountCodeToString(entries[0].DebitAccount)
            : "131";

        var cashTx = new CashTransaction
        {
            AccountingDate = saved.AccountingDate,
            DocumentDate   = saved.DocumentDate,
            ReceiptNumber  = null,
            PaymentNumber  = saved.DocumentNumber,
            Description    = saved.PayeeName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = 0m,
            CreditAmount   = totalAmount,  // Payment decreases cash
            PersonName     = saved.PayeeName,
            CreatedAt      = DateTime.UtcNow,
        };

        await _cashRepo.AddAsync(cashTx, ct);

        _logger.LogInformation("Created Payment {DocumentNumber} for supplier {SupplierId}",
            saved.DocumentNumber, saved.SupplierId);

        return GetPaymentsUseCase.MapToDto(saved);
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
