namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetNextDepositCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
