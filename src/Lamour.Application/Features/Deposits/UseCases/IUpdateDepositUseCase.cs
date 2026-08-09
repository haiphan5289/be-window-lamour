using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IUpdateDepositUseCase
{
    Task<DepositResponseDto> ExecuteAsync(int id, UpdateDepositRequestDto request, CancellationToken ct = default);
}
