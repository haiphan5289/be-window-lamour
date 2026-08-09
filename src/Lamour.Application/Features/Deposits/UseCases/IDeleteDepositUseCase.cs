namespace Lamour.Application.Features.Deposits.UseCases;

public interface IDeleteDepositUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
