namespace Lamour.Application.Features.Products.UseCases;

public interface IDeleteProductUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
