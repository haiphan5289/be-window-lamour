namespace Lamour.Application.Features.Warehouses.UseCases;

public interface IDeleteWarehouseUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
