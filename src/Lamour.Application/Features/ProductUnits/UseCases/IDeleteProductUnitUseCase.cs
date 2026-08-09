namespace Lamour.Application.Features.ProductUnits.UseCases;

public interface IDeleteProductUnitUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
