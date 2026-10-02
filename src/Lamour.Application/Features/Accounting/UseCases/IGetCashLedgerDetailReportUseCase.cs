using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetCashLedgerDetailReportUseCase
{
    Task<CashLedgerDetailReportDto> ExecuteAsync(
        DateTime from, DateTime to, IReadOnlyCollection<string> accountCodes,
        bool mergeSimilar, bool orderByCreated, CancellationToken ct = default);
}
