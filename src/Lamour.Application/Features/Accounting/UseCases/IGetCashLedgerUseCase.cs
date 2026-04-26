using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetCashLedgerUseCase
{
    Task<CashLedgerResponseDto> ExecuteAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
