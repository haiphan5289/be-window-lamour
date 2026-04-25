namespace Lamour.Application.Features.Customers.UseCases;

public interface IDeleteCustomerUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
