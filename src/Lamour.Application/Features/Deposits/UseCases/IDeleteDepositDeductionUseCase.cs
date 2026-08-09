namespace Lamour.Application.Features.Deposits.UseCases;

public interface IDeleteDepositDeductionUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
