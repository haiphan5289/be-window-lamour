namespace Lamour.Application.Features.Customers.UseCases;

public interface IGetNextCustomerCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
