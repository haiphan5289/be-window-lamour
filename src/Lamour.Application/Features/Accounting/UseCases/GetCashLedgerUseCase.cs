using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetCashLedgerUseCase : IGetCashLedgerUseCase
{
    private readonly ICashLedgerRepository _repo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ILogger<GetCashLedgerUseCase> _logger;

    public GetCashLedgerUseCase(
        ICashLedgerRepository repo,
        IPaymentRepository paymentRepo,
        ILogger<GetCashLedgerUseCase> logger)
    {
        _repo        = repo;
        _paymentRepo = paymentRepo;
        _logger      = logger;
    }

    public async Task<CashLedgerResponseDto> ExecuteAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching cash ledger from {From} to {To}", from, to);

        var openingBalance      = await _repo.GetBalanceBeforeDateAsync(from, ct);
        var transactions        = await _repo.GetByDateRangeAsync(from, to, ct);
        var unconfirmedPayments = await _paymentRepo.GetUnconfirmedByDateRangeAsync(from, to, ct);

        // Confirmed rows come from posted CashTransactions; Draft/Treo rows are Payments not
        // yet ghi số — shown for visibility only, they must not move the running balance.
        var rows = transactions
            .Select(t => new CashLedgerEntryDto
            {
                AccountingDate = t.AccountingDate,
                DocumentDate   = t.DocumentDate,
                ReceiptNumber  = t.ReceiptNumber,
                PaymentNumber  = t.PaymentNumber,
                Description    = t.Description,
                Account        = t.Account,
                CounterAccount = t.CounterAccount,
                DebitAmount    = t.DebitAmount,
                CreditAmount   = t.CreditAmount,
                Amount         = t.DebitAmount != 0m ? t.DebitAmount : t.CreditAmount,
                PersonName     = t.PersonName,
                PaymentReason  = t.PaymentReason,
                DocumentType   = t.DocumentType,
                Status         = "Confirmed",
            })
            .Concat(unconfirmedPayments
                .Where(p => p.Entries.Count > 0)
                .Select(p =>
                {
                    var totalAmount = p.Entries.Sum(e => e.Amount);
                    return new CashLedgerEntryDto
                    {
                        AccountingDate = p.AccountingDate,
                        DocumentDate   = p.DocumentDate,
                        ReceiptNumber  = null,
                        PaymentNumber  = p.DocumentNumber,
                        Description    = p.PayeeName,
                        Account        = "111",
                        CounterAccount = p.Entries.First().DebitAccountSetting.Code,
                        DebitAmount    = 0m,
                        CreditAmount   = totalAmount,
                        Amount         = totalAmount,
                        PersonName     = p.PayeeName,
                        PaymentReason  = p.PaymentReason.ToString(),
                        DocumentType   = "Phiếu chi",
                        Status         = p.Status.ToString(),
                    };
                }))
            .OrderBy(e => e.AccountingDate)
            .ToList();

        var runningBalance = openingBalance;
        foreach (var row in rows)
        {
            if (row.Status == "Confirmed")
                runningBalance += row.DebitAmount - row.CreditAmount;
            row.Balance = runningBalance;
        }

        return new CashLedgerResponseDto
        {
            OpeningBalance = openingBalance,
            ClosingBalance = rows.Count > 0 ? rows[^1].Balance : openingBalance,
            Entries        = rows,
        };
    }
}
