namespace Lamour.Application.Features.Accounting.UseCases;

public interface IDeletePaymentUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
