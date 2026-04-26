using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetCashLedgerUseCase : IGetCashLedgerUseCase
{
    private readonly ICashLedgerRepository _repo;
    private readonly ILogger<GetCashLedgerUseCase> _logger;

    public GetCashLedgerUseCase(ICashLedgerRepository repo, ILogger<GetCashLedgerUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<CashLedgerResponseDto> ExecuteAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching cash ledger from {From} to {To}", from, to);

        var openingBalance = await _repo.GetBalanceBeforeDateAsync(from, ct);
        var transactions   = await _repo.GetByDateRangeAsync(from, to, ct);

        var runningBalance = openingBalance;
        var entries = transactions.Select(t =>
        {
            runningBalance += t.DebitAmount - t.CreditAmount;
            return new CashLedgerEntryDto
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
                Balance        = runningBalance,
                PersonName     = t.PersonName,
            };
        }).ToList();

        return new CashLedgerResponseDto
        {
            OpeningBalance = openingBalance,
            ClosingBalance = entries.Count > 0 ? entries[^1].Balance : openingBalance,
            Entries        = entries,
        };
    }
}
