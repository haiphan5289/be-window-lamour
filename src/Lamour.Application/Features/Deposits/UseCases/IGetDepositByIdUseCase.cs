using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetDepositByIdUseCase
{
    Task<DepositResponseDto?> ExecuteAsync(int id, CancellationToken ct = default);
}
