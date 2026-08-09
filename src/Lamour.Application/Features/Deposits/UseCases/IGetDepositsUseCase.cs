using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetDepositsUseCase
{
    Task<IEnumerable<DepositResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
