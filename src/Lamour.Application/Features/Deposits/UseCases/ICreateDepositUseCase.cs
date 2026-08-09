using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface ICreateDepositUseCase
{
    Task<DepositResponseDto> ExecuteAsync(CreateDepositRequestDto request, CancellationToken ct = default);
}
