using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetDepositDeductionByIdUseCase
{
    Task<DepositDeductionResponseDto?> ExecuteAsync(int id, CancellationToken ct = default);
}
