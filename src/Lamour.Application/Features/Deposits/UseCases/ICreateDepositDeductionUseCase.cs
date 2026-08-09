using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface ICreateDepositDeductionUseCase
{
    Task<DepositDeductionResponseDto> ExecuteAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default);
}
