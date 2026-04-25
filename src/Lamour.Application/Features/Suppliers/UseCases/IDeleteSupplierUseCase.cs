namespace Lamour.Application.Features.Suppliers.UseCases;

public interface IDeleteSupplierUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
