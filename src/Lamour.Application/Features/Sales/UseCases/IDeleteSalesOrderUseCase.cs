namespace Lamour.Application.Features.Sales.UseCases;

public interface IDeleteSalesOrderUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
